function updateWishlistBadge(count) {
    const badge = document.getElementById('wishlistBadge');
    if (!badge) return;

    count = parseInt(count) || 0;
    badge.textContent = count;
    badge.style.display = count > 0 ? 'inline-block' : 'none';
}