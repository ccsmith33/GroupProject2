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
        <div id="app-landing" style="display: block;"></div>
        <div id="app-about" style="display: none;"></div>
        <div id="app-dashboard" class="dashboard-container" style="display: none;"></div>
        <div id="app-file-upload" class="file-upload-container" style="display: none;"></div>
        <div id="app-profile" class="profile-container" style="display: none;"></div>
        <div class="container mt-4">
          <div id="app-analysis" style="display: none;"></div>
        </div>
      </div>
    `;
  }

  initializeComponents() {
    try {
      // Initialize components
      this.components.header = new Header(document.getElementById("app-header"));
      
      const landingContainer = document.getElementById("app-landing");
      if (landingContainer) {
        this.components.landing = new LandingPage(landingContainer);
      }
      
      const aboutContainer = document.getElementById("app-about");
      if (aboutContainer) {
        this.components.about = new AboutPage(aboutContainer);
      }
      
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
    // Landing page navigation
    if (this.components.landing) {
      this.components.landing.on("navigateToPage", (pageName) => {
        this.switchToTab(pageName);
      });
    }

    // About page navigation
    if (this.components.about) {
      this.components.about.on("navigateToPage", (pageName) => {
        this.switchToTab(pageName);
      });
    }

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
    const sections = ['app-landing', 'app-about', 'app-dashboard', 'app-file-upload', 'app-analysis', 'app-profile'];
    sections.forEach(sectionId => {
      const section = document.getElementById(sectionId);
      if (section) {
        section.style.display = 'none';
      }
    });

    // Show the active section
    let activeSectionId;
    switch(activeTab) {
      case 'home':
        activeSectionId = 'app-landing';
        break;
      case 'about':
        activeSectionId = 'app-about';
        break;
      case 'study':
        activeSectionId = 'app-dashboard';
        break;
      case 'upload':
        activeSectionId = 'app-file-upload';
        break;
      case 'analysis':
        activeSectionId = 'app-analysis';
        break;
      case 'profile':
        activeSectionId = 'app-profile';
        break;
      default:
        activeSectionId = 'app-landing';
    }

    const activeSection = document.getElementById(activeSectionId);
    if (activeSection) {
      activeSection.style.display = 'block';
    }

    // Scroll to top when switching tabs
    window.scrollTo(0, 0);
  }

  switchToTab(tabName) {
    // Trigger tab change through header
    const event = new CustomEvent("tabChanged", {
      detail: { activeTab: tabName }
    });
    document.dispatchEvent(event);
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