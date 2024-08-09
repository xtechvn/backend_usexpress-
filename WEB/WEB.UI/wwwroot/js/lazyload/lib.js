// Check browser support lazyload image
if ('loading' in HTMLImageElement.prototype) {

    const images = document.querySelectorAll('img[loading="lazy"]');
    images.forEach(img => { img.src = img.dataset.src;

    });
} else {
    // NOT SUPPORT LAZY
    // Dynamically import the LazySizes library
    const script = document.createElement('script');
    script.src =
        'https://cdnjs.cloudflare.com/ajax/libs/lazysizes/4.1.8/lazysizes.min.js';
    document.body.appendChild(script);
}
