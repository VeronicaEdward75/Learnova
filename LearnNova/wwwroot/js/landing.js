document.addEventListener('DOMContentLoaded', () => {
    
    // 1. Navbar & Scroll Progress & Back to Top
    const navbar = document.getElementById('mainNav');
    const scrollBar = document.getElementById('scrollBar');
    const backToTopBtn = document.getElementById('backToTop');
    
    const updateScrollState = () => {
        const winScroll = document.body.scrollTop || document.documentElement.scrollTop;
        const height = document.documentElement.scrollHeight - document.documentElement.clientHeight;
        const scrolled = (winScroll / height) * 100;
        
        // Progress bar
        if(scrollBar) scrollBar.style.width = scrolled + "%";
        
        // Navbar shrink/glass
        if (winScroll > 50) {
            navbar.classList.add('scrolled');
        } else {
            navbar.classList.remove('scrolled');
        }

        // Back to top
        if (winScroll > 500) {
            backToTopBtn.classList.add('visible');
        } else {
            backToTopBtn.classList.remove('visible');
        }
    };
    
    window.addEventListener('scroll', updateScrollState);
    updateScrollState(); // init

    if(backToTopBtn) {
        backToTopBtn.addEventListener('click', () => {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        });
    }

    // 2. Mobile Menu
    const mobileBtn = document.getElementById('mobileMenuBtn');
    const mobileMenu = document.getElementById('mobileMenu');
    const closeMenuBtn = document.getElementById('closeMenuBtn');
    
    if (mobileBtn && mobileMenu) {
        mobileBtn.addEventListener('click', () => {
            mobileMenu.style.display = 'block';
            setTimeout(() => { mobileMenu.style.opacity = '1'; }, 10);
        });
        
        const closeMenu = () => {
            mobileMenu.style.opacity = '0';
            setTimeout(() => { mobileMenu.style.display = 'none'; }, 300);
        };
        
        closeMenuBtn.addEventListener('click', closeMenu);
        document.querySelectorAll('.mobile-nav-link').forEach(link => {
            link.addEventListener('click', closeMenu);
        });
    }

    // 3. Advanced Scroll Reveal
    const revealElements = document.querySelectorAll('.reveal');
    const revealObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                // Add staggered delay if inside a grid
                const delay = entry.target.getAttribute('data-delay') || '0';
                entry.target.style.animationDelay = delay + 's';
                entry.target.classList.add('active');
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.15, rootMargin: "0px 0px -50px 0px" });
    
    revealElements.forEach(el => revealObserver.observe(el));

    // 4. Animated Counters
    const counters = document.querySelectorAll('.stat-val .counter');
    let countersAnimated = false;
    
    const animateCounters = () => {
        counters.forEach(counter => {
            const target = +counter.getAttribute('data-target');
            const duration = 2500;
            const increment = target / (duration / 16);
            
            let current = 0;
            const updateCounter = () => {
                current += increment;
                if (current < target) {
                    counter.innerText = Math.ceil(current);
                    requestAnimationFrame(updateCounter);
                } else {
                    counter.innerText = target;
                }
            };
            updateCounter();
        });
    };

    const trustSection = document.querySelector('.trust-section');
    if (trustSection) {
        const statsObserver = new IntersectionObserver((entries) => {
            if (entries[0].isIntersecting && !countersAnimated) {
                countersAnimated = true;
                animateCounters();
            }
        }, { threshold: 0.5 });
        statsObserver.observe(trustSection);
    }

    // 5. Testimonials Carousel
    const track = document.getElementById('carouselTrack');
    const prevBtn = document.getElementById('prevTesti');
    const nextBtn = document.getElementById('nextTesti');
    
    if (track && prevBtn && nextBtn) {
        let position = 0;
        // Approximation: card width + gap
        const step = 424; 
        
        // Very basic custom slider logic
        nextBtn.addEventListener('click', () => {
            // max scroll depends on children. We'll just allow a few steps
            const maxScroll = (track.children.length - 1) * step;
            if (Math.abs(position) < maxScroll) {
                position -= step;
                track.style.transform = `translateX(${position}px)`; // RTL note: translateX negative goes left, which means next in LTR, but in RTL might be reversed depending on layout. Since we used flex without rtl direction explicit on track, we'll see. Actually in RTL, translateX positive goes right.
                // We'll just translate positive to go "next" in RTL.
                position = Math.abs(position); // Let's simplify
            }
        });
        
        // RTL adjustment
        let currentIdx = 0;
        const totalItems = track.children.length;
        
        const updateCarousel = () => {
            // 400px card + 24px gap = 424px
            track.style.transform = `translateX(${currentIdx * 424}px)`;
        };

        nextBtn.addEventListener('click', () => {
            if (currentIdx < totalItems - 1) {
                currentIdx++;
                updateCarousel();
            }
        });

        prevBtn.addEventListener('click', () => {
            if (currentIdx > 0) {
                currentIdx--;
                updateCarousel();
            }
        });
    }

    // 6. FAQ Accordion
    const faqItems = document.querySelectorAll('.faq-item-v2');
    faqItems.forEach(item => {
        const head = item.querySelector('.faq-head-v2');
        const body = item.querySelector('.faq-body-v2');
        
        head.addEventListener('click', () => {
            const isActive = item.classList.contains('active');
            
            // Close others
            faqItems.forEach(other => {
                other.classList.remove('active');
                other.querySelector('.faq-body-v2').style.maxHeight = null;
            });
            
            if (!isActive) {
                item.classList.add('active');
                body.style.maxHeight = body.scrollHeight + "px";
            }
        });
    });

    // 7. Ripple Effect for Buttons
    const rippleButtons = document.querySelectorAll('.ripple');
    rippleButtons.forEach(btn => {
        btn.addEventListener('mousedown', function(e) {
            let x = e.clientX - e.target.getBoundingClientRect().left;
            let y = e.clientY - e.target.getBoundingClientRect().top;
            let ripple = document.createElement('span');
            ripple.style.position = 'absolute';
            ripple.style.background = 'rgba(255,255,255,0.3)';
            ripple.style.borderRadius = '50%';
            ripple.style.transform = 'translate(-50%, -50%)';
            ripple.style.pointerEvents = 'none';
            ripple.style.left = `${x}px`;
            ripple.style.top = `${y}px`;
            ripple.style.width = '0px';
            ripple.style.height = '0px';
            ripple.style.transition = 'all 0.5s ease-out';
            
            this.appendChild(ripple);
            
            setTimeout(() => {
                ripple.style.width = '300px';
                ripple.style.height = '300px';
                ripple.style.opacity = '0';
            }, 10);
            
            setTimeout(() => {
                ripple.remove();
            }, 500);
        });
    });
});
