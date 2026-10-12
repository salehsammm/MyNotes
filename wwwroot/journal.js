export function openEpisode(dialog) {
    if (!dialog.open) dialog.showModal();
    // Blazor updates the dialog fields after the click handler finishes.
    requestAnimationFrame(() => {
        const rating = dialog.querySelector('#episode-rating');
        rating?.focus();
        rating?.select();
    });
}

export function closeEpisode(dialog) {
    if (dialog.open) dialog.close();
}
