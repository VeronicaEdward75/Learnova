using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LearnNova.Models.Entities;

namespace LearnNova.Services;

public interface ICouponService
{
    Task<(bool IsValid, string Message, Coupon? Coupon)> ValidateCouponAsync(string code, int courseId, string studentId);
    decimal CalculateDiscount(decimal price, Coupon coupon);
    Task RecordCouponUsageAsync(int couponId, string studentId, int paymentId);
    
    Task<IEnumerable<Coupon>> GetAllCouponsAsync();
    Task<IEnumerable<Coupon>> GetTeacherCouponsAsync(string teacherId);
    Task<Coupon?> GetCouponByIdAsync(int id);
    Task<Coupon?> GetCouponByCodeAsync(string code);
    
    Task<(bool Success, string Message)> CreateCouponAsync(Coupon coupon);
    Task<(bool Success, string Message)> UpdateCouponAsync(Coupon coupon);
    Task<(bool Success, string Message)> ToggleCouponStatusAsync(int id);
    Task<(bool Success, string Message)> DeleteCouponAsync(int id);
    Task<int> GetCouponUsageCountAsync(int couponId);
}
