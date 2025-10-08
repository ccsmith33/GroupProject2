// Main App component that handles the overall application structure
class App {
  constructor() {
    this.container = document.createElement("div");
    this.components = {};
    this.init();
  }

  init() {
    this.render();
    // Wait for DOM to be ready before initializing components
    setTimeout(() => {
      this.initializeComponents();
    }, 0);
  }

  render() {
    this.container.innerHTML = `
      <div class="container-fluid">
        <div id="app-header"></div>
        <div class="container mt-4">
          <div id="app-dashboard"></div>
          <div id="app-file-upload" style="display: none;"></div>
          <div id="app-analysis" style="display: none;"></div>
          <div id="app-profile" style="display: none;"></div>
        </div>
      </div>
    `;
  }

  initializeComponents() {
    try {
      // Initialize components
      this.components.header = new Header(document.getElementById("app-header"));
      this.components.dashboard = new Dashboard(document.getElementById("app-dashboard"));
      this.components.fileUpload = new FileUpload(document.getElementById("app-file-upload"));
      this.components.analysis = new Analysis(document.getElementById("app-analysis"));
      this.components.profile = new Profile(document.getElementById("app-profile"));

      // Setup component communication
      this.setupComponentCommunication();

      console.log("All components initialized successfully");
    } catch (error) {
      console.error("Failed to initialize components:", error);
    }
  }

  setupComponentCommunication() {
    // File upload events
    this.components.fileUpload.on("filesUploaded", (files) => {
      this.components.analysis.enableAnalysis();
    });

    this.components.fileUpload.on("analysisRequested", (data) => {
      this.components.analysis.startAnalysis(data);
    });

    // Analysis events
    this.components.analysis.on("analysisComplete", (results) => {
      this.components.profile.updateWithResults(results);
    });

    // Tab change events
    document.addEventListener("tabChanged", (e) => {
      this.handleTabChange(e.detail.activeTab);
    });
  }

  handleTabChange(activeTab) {
    // Hide all sections
    const sections = ['app-dashboard', 'app-file-upload', 'app-analysis', 'app-profile'];
    sections.forEach(sectionId => {
      const section = document.getElementById(sectionId);
      if (section) {
        section.style.display = 'none';
      }
    });

    // Show the active section
    let activeSectionId;
    switch(activeTab) {
      case 'study':
        activeSectionId = 'app-dashboard';
        break;
      case 'upload':
        activeSectionId = 'app-file-upload';
        break;
      case 'profile':
        activeSectionId = 'app-profile';
        break;
      default:
        activeSectionId = 'app-dashboard';
    }

    const activeSection = document.getElementById(activeSectionId);
    if (activeSection) {
      activeSection.style.display = 'block';
    }

    // Scroll to top when switching tabs
    window.scrollTo(0, 0);
  }

  // Global event handling
  handleGlobalEvents() {
    // Handle login/logout
    document.addEventListener("click", (e) => {
      if (e.target.id === "loginBtn") {
        this.handleLogin();
      }
    });
  }

  handleLogin() {
    const loginBtn = document.getElementById("loginBtn");
    if (loginBtn) {
      if (loginBtn.textContent === "Login") {
        loginBtn.textContent = "Logout";
        this.showNotification("Logged in successfully", "success");
      } else {
        loginBtn.textContent = "Login";
        this.showNotification("Logged out successfully", "info");
      }
    }
  }

  showNotification(message, type = "info") {
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
  }
}