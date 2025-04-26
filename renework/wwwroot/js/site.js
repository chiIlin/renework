// Add this to site.js or create a new JS file

document.addEventListener('DOMContentLoaded', function() {
    // Function to animate counter
    function animateCounter() {
        // Set target value (you can change this later)
        const targetValue = 123456789; // Example value: 123,456,789
        
        // Get all digit elements
        const digitElements = document.querySelectorAll('.digit');
        
        // Start from 0
        let currentValue = 0;
        
        // Animation duration in milliseconds
        const duration = 3000;
        const steps = 100;
        const increment = targetValue / steps;
        const stepDuration = duration / steps;
        
        // Animation loop
        const animation = setInterval(() => {
            currentValue += increment;
            
            if (currentValue >= targetValue) {
                currentValue = targetValue;
                clearInterval(animation);
            }
            
            // Convert current value to string with padded zeros
            const valueString = Math.floor(currentValue).toString().padStart(9, '0');
            
            // Update each digit element
            for (let i = 0; i < digitElements.length; i++) {
                digitElements[i].textContent = valueString[i];
            }
        }, stepDuration);
    }
    
    // Observe when counter section comes into view
    const counterSection = document.querySelector('.counter-container');
    if (counterSection) {
        // Use Intersection Observer to trigger animation when element is in view
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    animateCounter();
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.2 });
        
        observer.observe(counterSection);
    }
});