// Main application logic
document.addEventListener("DOMContentLoaded", function () {
  console.log("Student Study AI Platform loaded");

  // Initialize application
  initializeApp();

  // Setup global event listeners
  setupGlobalEventListeners();
});

function initializeApp() {
  // Check if we're in development mode (no backend running)
  checkBackendStatus();

  // Initialize any other components
  initializeComponents();
}

function setupGlobalEventListeners() {
  // Login button
  const loginBtn = document.getElementById("loginBtn");
  if (loginBtn) {
    loginBtn.addEventListener("click", handleLogin);
  }

  // Any other global event listeners can be added here
}

function initializeComponents() {
  // Components are initialized in their own files
  // This is where we could initialize additional components
  console.log("Components initialized");
}

async function checkBackendStatus() {
  try {
    const response = await fetch("http://localhost:5000/health");
    if (response.ok) {
      console.log("Backend is running");
      return true;
    }
  } catch (error) {
    console.log("Backend not available, using mock data");
    // Backend is not running, we'll use mock data
  }
  return false;
}

function handleLogin() {
  // Simple login simulation
  const loginBtn = document.getElementById("loginBtn");
  if (loginBtn) {
    loginBtn.textContent = "Logout";
    loginBtn.onclick = handleLogout;
  }

  // Show user is logged in
  showNotification("Logged in successfully", "success");
}

function handleLogout() {
  const loginBtn = document.getElementById("loginBtn");
  if (loginBtn) {
    loginBtn.textContent = "Login";
    loginBtn.onclick = handleLogin;
  }

  // Clear any user data
  showNotification("Logged out successfully", "info");
}

function showNotification(message, type = "info") {
  // Simple notification system
  const notification = document.createElement("div");
  notification.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
  notification.style.cssText =
    "top: 20px; right: 20px; z-index: 9999; min-width: 300px;";
  notification.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

  document.body.appendChild(notification);

  // Auto remove after 3 seconds
  setTimeout(() => {
    if (notification.parentNode) {
      notification.parentNode.removeChild(notification);
    }
  }, 3000);
}

// Utility functions
function formatDate(dateString) {
  const date = new Date(dateString);
  return date.toLocaleDateString() + " " + date.toLocaleTimeString();
}

function debounce(func, wait) {
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

// Export functions for use in other modules
window.StudentStudyAI = {
  showNotification,
  formatDate,
  debounce,
};
