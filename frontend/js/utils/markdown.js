// Simple markdown parser for study guide content
export class MarkdownParser {
  static parse(markdown) {
    if (!markdown) return "";

    let html = markdown
      // Headers
      .replace(/^### (.*$)/gim, "<h3 class='study-guide-h3'>$1</h3>")
      .replace(/^## (.*$)/gim, "<h2 class='study-guide-h2'>$1</h2>")
      .replace(/^# (.*$)/gim, "<h1 class='study-guide-h1'>$1</h1>")

      // Bold text
      .replace(/\*\*(.*?)\*\*/g, "<strong class='study-guide-bold'>$1</strong>")

      // Italic text
      .replace(/\*(.*?)\*/g, "<em class='study-guide-italic'>$1</em>")

      // Bullet points
      .replace(/^\* (.*$)/gim, "<li class='study-guide-li'>$1</li>")
      .replace(/^- (.*$)/gim, "<li class='study-guide-li'>$1</li>")

      // Numbered lists
      .replace(/^\d+\. (.*$)/gim, "<li class='study-guide-li'>$1</li>")

      // Line breaks
      .replace(/\n\n/g, "</p><p class='study-guide-p'>")
      .replace(/\n/g, "<br>");

    // Wrap list items in ul tags
    html = html.replace(/(<li class='study-guide-li'>.*<\/li>)/gs, (match) => {
      return `<ul class='study-guide-ul'>${match}</ul>`;
    });

    // Wrap paragraphs
    html = `<p class='study-guide-p'>${html}</p>`;

    // Clean up empty paragraphs
    html = html.replace(/<p class='study-guide-p'><\/p>/g, "");
    html = html.replace(/<p class='study-guide-p'><br><\/p>/g, "");

    return html;
  }

  static formatForDisplay(content) {
    const parsed = this.parse(content);
    return `<div class='study-guide-content'>${parsed}</div>`;
  }
}

// Helper function to create download link
export function createDownloadLink(blob, filename) {
  const url = window.URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  window.URL.revokeObjectURL(url);
}

// Helper function to format file size
export function formatFileSize(bytes) {
  if (bytes === 0) return "0 Bytes";
  const k = 1024;
  const sizes = ["Bytes", "KB", "MB", "GB"];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
}

// Helper function to get file icon
export function getFileIcon(fileType) {
  const extension = fileType.toLowerCase().replace(".", "");
  const icons = {
    pdf: "bi-file-earmark-pdf",
    doc: "bi-file-earmark-word",
    docx: "bi-file-earmark-word",
    ppt: "bi-file-earmark-ppt",
    pptx: "bi-file-earmark-ppt",
    txt: "bi-file-earmark-text",
    jpg: "bi-file-earmark-image",
    jpeg: "bi-file-earmark-image",
    png: "bi-file-earmark-image",
  };
  return icons[extension] || "bi-file-earmark";
}
