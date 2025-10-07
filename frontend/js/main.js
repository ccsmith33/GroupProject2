// Main application entry point
// Initialize the application when DOM is loaded
document.addEventListener("DOMContentLoaded", function () {
  console.log("Data Driven Studying Platform loading...");

  try {
    // Create and initialize the main app
    const app = new App();
    
    // Mount the app to the root element
    const appRoot = document.getElementById("app-root");
    if (appRoot) {
      appRoot.appendChild(app.container);
    }

    console.log("Data Driven Studying Platform loaded successfully");
  } catch (error) {
    console.error("Failed to initialize application:", error);
    
    // Show error message to user
    const appRoot = document.getElementById("app-root");
    if (appRoot) {
      appRoot.innerHTML = `
        <div class="container-fluid">
          <div class="alert alert-danger" role="alert">
            <h4 class="alert-heading">Application Error</h4>
            <p>Failed to load the application. Please refresh the page and try again.</p>
            <hr>
            <p class="mb-0">If the problem persists, please check the browser console for more details.</p>
          </div>
        </div>
      `;
    }
  }
});

// Global utility functions
window.StudentStudyAI = {
  showNotification: function(message, type = "info") {
    const notification = document.createElement("div");
    notification.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
    notification.style.cssText = "top: 20px; right: 20px; z-index: 9999; min-width: 300px;";
    notification.innerHTML = `
      ${message}
      <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

    document.body.appendChild(notification);

    setTimeout(() => {
      if (notification.parentNode) {
        notification.parentNode.removeChild(notification);
      }
    }, 3000);
  },

  formatDate: function(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString() + " " + date.toLocaleTimeString();
  },

  debounce: function(func, wait) {
    let timeout;
    return function executedFunction(...args) {
      const later = () => {
        clearTimeout(timeout);
        func(...args);
      };
      clearTimeout(timeout);
      timeout = setTimeout(later, wait);
    };
  }
};
