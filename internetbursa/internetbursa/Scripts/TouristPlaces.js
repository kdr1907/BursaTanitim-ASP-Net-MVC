
    function toggleLocation(yerAd) {
        var locationText = document.getElementById('location-' + yerAd);
    if (locationText.style.display === 'none') {
        locationText.style.display = 'block';  // Konum bilgisini göster
        } else {
        locationText.style.display = 'none';  // Konum bilgisini gizle
        }
    }
