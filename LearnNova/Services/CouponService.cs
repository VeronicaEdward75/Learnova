using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LearnNova.Models.Entities;
using LearnNova.Models.Enums;
using LearnNova.Repositories;

namespace LearnNova.Services;

public class CouponService : ICouponService
{
    private readonly IGenericRepository<Coupon> _couponRepository;
    private readonly IGenericRepository<CouponUsage> _usageRepository;
    private readonly ICourseRepository _courseRepository;

    public CouponService(
        IGenericRepository<Coupon> couponRepository,
        IGenericRepository<CouponUsage> usageRepository,
        ICourseRepository courseRepository)
    {
        _couponRepository = couponRepository;
        _usageRepository = usageRepository;
        _courseRepository = courseRepository;
    }

    public async Task<(bool IsValid, string Message, Coupon? Coupon)> ValidateCouponAsync(string code, int courseId, string studentId)
    {
        if (string.IsNullOrWhiteSpace(code)) return (false, "كود الكوبون فارغ", null);

        var coupons = await _couponRepository.FindAsync(c => c.Code.ToLower() == code.ToLower());
        var coupon = coupons.FirstOrDefault();

        if (coupon == null) return (false, "كود الكوبون غير موجود", null);
        if (!coupon.IsActive) return (false, "كود الكوبون غير موجود", null);
        if (coupon.StartDate.HasValue && coupon.StartDate.Value > DateTime.UtcNow) return (false, "هذا الكوبون لم يبدأ بعد", null);
        if (coupon.EndDate.HasValue && coupon.EndDate.Value < DateTime.UtcNow) return (false, "هذا الكوبون منتهي الصلاحية", null);

        var usages = await _usageRepository.FindAsync(u => u.CouponId == coupon.Id);
        if (coupon.UsageLimit.HasValue && usages.Count() >= coupon.UsageLimit.Value) return (false, "لقد تم تجاوز الحد الأقصى لاستخدام الكوبون", null);

        // Check if user already used it
        if (usages.Any(u => u.StudentId == studentId)) return (false, "لقد قمت باستخدام هذا الكوبون مسبقاً", null);

        // Scope validation
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null) return (false, "الكورس غير موجود", null);

        if (coupon.CourseId.HasValue && coupon.CourseId.Value != courseId) return (false, "هذا الكوبون غير صالح لهذا الكورس", null);
        if (!string.IsNullOrEmpty(coupon.TeacherId) && coupon.TeacherId != course.TeacherId) return (false, "هذا الكوبون غير صالح لكورس هذا المعلم", null);

        if (coupon.MinimumOrder.HasValue && course.Price < coupon.MinimumOrder.Value) return (false, $"هذا الكوبون يتطلب حد أدنى للطلب {coupon.MinimumOrder.Value} EGP", null);

        return (true, "كود الكوبون فارغ", coupon);
    }

    public decimal CalculateDiscount(decimal price, Coupon coupon)
    {
        decimal discount = 0;
        
        switch (coupon.DiscountType)
        {
            case DiscountType.Percentage:
                discount = price * (coupon.DiscountValue / 100m);
                if (coupon.MaximumDiscount.HasValue && discount > coupon.MaximumDiscount.Value)
                {
                    discount = coupon.MaximumDiscount.Value;
                }
                break;
            case DiscountType.FixedAmount:
                discount = coupon.DiscountValue;
                break;
            case DiscountType.FreeCourse:
                discount = price;
                break;
        }

        return discount > price ? price : discount;
    }

    public async Task RecordCouponUsageAsync(int couponId, string studentId, int paymentId)
    {
        var usage = new CouponUsage
        {
            CouponId = couponId,
            StudentId = studentId,
            PaymentId = paymentId,
            UsedAt = DateTime.UtcNow
        };
        await _usageRepository.AddAsync(usage);
    }

    public async Task<IEnumerable<Coupon>> GetAllCouponsAsync()
    {
        return await _couponRepository.GetAllAsync();
    }

    public async Task<IEnumerable<Coupon>> GetTeacherCouponsAsync(string teacherId)
    {
        var all = await _couponRepository.GetAllAsync();
        return all.Where(c => c.TeacherId == teacherId).ToList();
    }

    public async Task<Coupon?> GetCouponByIdAsync(int id)
    {
        return await _couponRepository.GetByIdAsync(id);
    }

    public async Task<Coupon?> GetCouponByCodeAsync(string code)
    {
        var coupons = await _couponRepository.FindAsync(c => c.Code.ToLower() == code.ToLower());
        return coupons.FirstOrDefault();
    }

    public async Task<(bool Success, string Message)> CreateCouponAsync(Coupon coupon)
    {
        var existing = await GetCouponByCodeAsync(coupon.Code);
        if (existing != null) return (false, "كود الكوبون موجود بالفعل");

        await _couponRepository.AddAsync(coupon);
        return (true, "تم إضافة هذا الكوبون بنجاح");
    }

    public async Task<(bool Success, string Message)> UpdateCouponAsync(Coupon coupon)
    {
        var existing = await _couponRepository.GetByIdAsync(coupon.Id);
        if (existing == null) return (false, "الكوبون غير موجود");

        var checkCode = await GetCouponByCodeAsync(coupon.Code);
        if (checkCode != null && checkCode.Id != coupon.Id) return (false, "كود الكوبون موجود بالفعل");

        existing.Code = coupon.Code;
        existing.DiscountType = coupon.DiscountType;
        existing.DiscountValue = coupon.DiscountValue;
        existing.StartDate = coupon.StartDate;
        existing.EndDate = coupon.EndDate;
        existing.MinimumOrder = coupon.MinimumOrder;
        existing.MaximumDiscount = coupon.MaximumDiscount;
        existing.UsageLimit = coupon.UsageLimit;
        existing.CourseId = coupon.CourseId;
        existing.IsActive = coupon.IsActive;

        _couponRepository.Update(existing);
        await _couponRepository.SaveChangesAsync();

        return (true, "تم تعديل الكوبون بنجاح");
    }

    public async Task<(bool Success, string Message)> ToggleCouponStatusAsync(int id)
    {
        var coupon = await _couponRepository.GetByIdAsync(id);
        if (coupon == null) return (false, "الكوبون غير موجود");

        coupon.IsActive = !coupon.IsActive;
        _couponRepository.Update(coupon);
        await _couponRepository.SaveChangesAsync();

        return (true, coupon.IsActive ? "تم تعديل الكوبون بنجاح" : "تم تعديل الكوبون بنجاح");
    }

    public async Task<(bool Success, string Message)> DeleteCouponAsync(int id)
    {
        var coupon = await _couponRepository.GetByIdAsync(id);
        if (coupon == null) return (false, "الكوبون غير موجود");

        _couponRepository.Remove(coupon);
        await _couponRepository.SaveChangesAsync();
        return (true, "تم حذف الكوبون بنجاح");
    }

    public async Task<int> GetCouponUsageCountAsync(int couponId)
    {
        var usages = await _usageRepository.FindAsync(u => u.CouponId == couponId);
        return usages.Count();
    }
}

