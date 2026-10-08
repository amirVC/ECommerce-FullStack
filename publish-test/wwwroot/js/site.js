function updateWishlistBadge(isWishlisted) {
    const badge = document.getElementById('wishlistBadge');
    if (!badge) return;

    let count = parseInt(badge.textContent.trim()) || 0;
    count = isWishlisted ? count + 1 : Math.max(0, count - 1);

    if (count > 0) {
        badge.textContent = count;
        badge.style.display = 'inline-block';
    } else {
        badge.style.display = 'none';
    }
}