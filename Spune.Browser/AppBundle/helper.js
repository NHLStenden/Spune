function spuneDownloadFile(fileName, fileMimeType, fileContents) {
    let blob = new Blob([fileContents], {type: fileMimeType});
    let link = document.createElement('a');
    link.href = window.URL.createObjectURL(blob);
    link.download = fileName;
    link.click();
}