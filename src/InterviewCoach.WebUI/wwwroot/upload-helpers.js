// Helper for triggering file input clicks from Blazor
window.triggerClick = function (elem) {
    elem.click();
};

// Triggers a browser download of text content as a file
window.downloadTextFile = function (filename, content) {
    const blob = new Blob([content], { type: 'text/markdown;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};
