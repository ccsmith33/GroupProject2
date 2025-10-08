// Profile component for user profile and study history
class Profile {
  constructor(container) {
    this.container = container;
    this.studySessions = [];
    this.recentFiles = [];
    this.userData = null;
    this.init();
  }

  init() {
    this.render();
    this.loadData();
    this.setupEventListeners();
  }

  render() {
    this.container.innerHTML = `
      <div class="row">
        <div class="col-12">
          <h1>Profile</h1>
          <p class="lead">Manage your account and view your study history</p>
        </div>
      </div>

      <!-- User Information -->
      <div class="row mt-4">
        <div class="col-md-6">
          <div class="card">
            <div class="card-header">
              <h5>Account Information</h5>
            </div>
            <div class="card-body">
              <div id="userInfo">
                <p class="text-muted">Please log in to view your profile information.</p>
              </div>
            </div>
          </div>
        </div>
        <div class="col-md-6">
          <div class="card">
            <div class="card-header">
              <h5>Account Settings</h5>
            </div>
            <div class="card-body">
              <div class="mb-3">
                <label for="profileName" class="form-label">Full Name</label>
                <input type="text" class="form-control" id="profileName" disabled>
              </div>
              <div class="mb-3">
                <label for="profileEmail" class="form-label">Email</label>
                <input type="email" class="form-control" id="profileEmail" disabled>
              </div>
              <div class="mb-3">
                <label for="profileSchool" class="form-label">School</label>
                <input type="text" class="form-control" id="profileSchool" disabled>
              </div>
              <button class="btn btn-primary" id="editProfileBtn" disabled>Edit Profile</button>
            </div>
          </div>
        </div>
      </div>

      <!-- Study Statistics -->
      <div class="row mt-4" id="studyStats">
        <div class="col-md-3">
          <div class="card text-center">
            <div class="card-body">
              <h5 class="card-title text-primary" id="totalSessions">0</h5>
              <p class="card-text">Study Sessions</p>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="card text-center">
            <div class="card-body">
              <h5 class="card-title text-success" id="totalFiles">0</h5>
              <p class="card-text">Files Analyzed</p>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="card text-center">
            <div class="card-body">
              <h5 class="card-title text-info" id="avgScore">0</h5>
              <p class="card-text">Average Score</p>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="card text-center">
            <div class="card-body">
              <h5 class="card-title text-warning" id="improvement">0%</h5>
              <p class="card-text">Improvement</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Recent Activity -->
      <div class="row mt-4">
        <div class="col-md-6">
          <div class="card">
            <div class="card-header">
              <h5>Recent Study Sessions</h5>
            </div>
            <div class="card-body">
              <div id="recentSessions">
                <p class="text-muted">No recent sessions</p>
              </div>
            </div>
          </div>
        </div>
        <div class="col-md-6">
          <div class="card">
            <div class="card-header">
              <h5>Recent Files</h5>
            </div>
            <div class="card-body">
              <div id="recentFilesList">
                <p class="text-muted">No recent files</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    `;
  }

  setupEventListeners() {
    // Listen for authentication state changes
    document.addEventListener("authStateChanged", (e) => {
      this.handleAuthStateChange(e.detail.isLoggedIn);
    });

    // Listen for tab changes
    document.addEventListener("tabChanged", (e) => {
      if (e.detail.activeTab === "profile") {
        this.loadData();
      }
    });

    // Edit profile button
    const editProfileBtn = document.getElementById("editProfileBtn");
    if (editProfileBtn) {
      editProfileBtn.addEventListener("click", () => {
        this.toggleEditMode();
      });
    }
  }

  handleAuthStateChange(isLoggedIn) {
    const userInfoDiv = document.getElementById("userInfo");
    const editProfileBtn = document.getElementById("editProfileBtn");
    
    if (isLoggedIn) {
      this.loadUserData();
      if (editProfileBtn) {
        editProfileBtn.disabled = false;
      }
    } else {
      if (userInfoDiv) {
        userInfoDiv.innerHTML = '<p class="text-muted">Please log in to view your profile information.</p>';
      }
      if (editProfileBtn) {
        editProfileBtn.disabled = true;
      }
      this.clearProfileData();
    }
  }

  loadUserData() {
    const userData = sessionStorage.getItem("userData") || localStorage.getItem("userData");
    if (userData) {
      this.userData = JSON.parse(userData);
      this.updateUserInfo();
    }
  }

  updateUserInfo() {
    if (!this.userData) return;

    const userInfoDiv = document.getElementById("userInfo");
    const profileName = document.getElementById("profileName");
    const profileEmail = document.getElementById("profileEmail");
    const profileSchool = document.getElementById("profileSchool");

    if (userInfoDiv) {
      userInfoDiv.innerHTML = `
        <div class="d-flex align-items-center mb-3">
          <div class="me-3">
            <div class="bg-primary rounded-circle d-flex align-items-center justify-content-center" style="width: 60px; height: 60px;">
              <span class="text-white fw-bold fs-4">${this.userData.name.charAt(0).toUpperCase()}</span>
            </div>
          </div>
          <div>
            <h5 class="mb-1">${this.userData.name}</h5>
            <p class="text-muted mb-0">${this.userData.email}</p>
            <small class="text-muted">${this.userData.school}</small>
          </div>
        </div>
      `;
    }

    if (profileName) profileName.value = this.userData.name;
    if (profileEmail) profileEmail.value = this.userData.email;
    if (profileSchool) profileSchool.value = this.userData.school;
  }

  clearProfileData() {
    const profileName = document.getElementById("profileName");
    const profileEmail = document.getElementById("profileEmail");
    const profileSchool = document.getElementById("profileSchool");

    if (profileName) profileName.value = "";
    if (profileEmail) profileEmail.value = "";
    if (profileSchool) profileSchool.value = "";
  }

  toggleEditMode() {
    const profileName = document.getElementById("profileName");
    const profileEmail = document.getElementById("profileEmail");
    const profileSchool = document.getElementById("profileSchool");
    const editProfileBtn = document.getElementById("editProfileBtn");

    const isEditing = !profileName.disabled;

    profileName.disabled = !isEditing;
    profileEmail.disabled = !isEditing;
    profileSchool.disabled = !isEditing;

    if (editProfileBtn) {
      editProfileBtn.textContent = isEditing ? "Edit Profile" : "Save Changes";
      editProfileBtn.className = isEditing ? "btn btn-primary" : "btn btn-success";
    }
  }

  async loadData() {
    try {
      // Load study sessions and files from localStorage or API
      this.studySessions = this.getStoredSessions();
      this.recentFiles = this.getStoredFiles();
      this.updateDisplay();
    } catch (error) {
      console.error("Error loading profile data:", error);
    }
  }

  getStoredSessions() {
    const stored = localStorage.getItem("studySessions");
    return stored ? JSON.parse(stored) : [];
  }

  getStoredFiles() {
    const stored = localStorage.getItem("recentFiles");
    return stored ? JSON.parse(stored) : [];
  }

  updateDisplay() {
    // Update statistics
    document.getElementById("totalSessions").textContent = this.studySessions.length;
    document.getElementById("totalFiles").textContent = this.recentFiles.length;

    const avgScore = this.studySessions.length > 0
      ? Math.round(this.studySessions.reduce((sum, session) => sum + session.score, 0) / this.studySessions.length)
      : 0;
    document.getElementById("avgScore").textContent = avgScore;

    // Update recent sessions
    this.renderRecentSessions();

    // Update recent files
    this.renderRecentFiles();
  }

  renderRecentSessions() {
    const container = document.getElementById("recentSessions");
    if (this.studySessions.length === 0) {
      container.innerHTML = "<p class=\"text-muted\">No recent sessions</p>";
      return;
    }

    const recentSessions = this.studySessions.slice(-5).reverse();
    container.innerHTML = recentSessions.map(session => `
      <div class="d-flex justify-content-between align-items-center mb-2">
        <div>
          <small class="text-muted">${new Date(session.date).toLocaleDateString()}</small>
          <div>${session.subject}</div>
        </div>
        <span class="badge bg-${this.getScoreColor(session.score)}">${session.score}%</span>
      </div>
    `).join("");
  }

  renderRecentFiles() {
    const container = document.getElementById("recentFilesList");
    if (this.recentFiles.length === 0) {
      container.innerHTML = "<p class=\"text-muted\">No recent files</p>";
      return;
    }

    const recentFiles = this.recentFiles.slice(-5).reverse();
    container.innerHTML = recentFiles.map(file => `
      <div class="d-flex justify-content-between align-items-center mb-2">
        <div>
          <small class="text-muted">${new Date(file.uploadDate).toLocaleDateString()}</small>
          <div>${file.name}</div>
        </div>
        <span class="badge bg-secondary">${file.type}</span>
      </div>
    `).join("");
  }

  getScoreColor(score) {
    if (score >= 80) return "success";
    if (score >= 60) return "warning";
    return "danger";
  }

  updateWithResults(results) {
    // Add new session to the list
    const newSession = {
      id: Date.now(),
      date: new Date().toISOString(),
      subject: results.subject || "Unknown",
      score: results.overallScore || 0,
      files: results.files || []
    };

    this.studySessions.push(newSession);
    localStorage.setItem("studySessions", JSON.stringify(this.studySessions));

    // Add files to recent files
    if (results.files) {
      results.files.forEach(file => {
        const fileEntry = {
          name: file.name,
          type: file.type,
          uploadDate: new Date().toISOString()
        };
        this.recentFiles.push(fileEntry);
      });
      localStorage.setItem("recentFiles", JSON.stringify(this.recentFiles));
    }

    this.updateDisplay();
  }

  updateProgress() {
    this.loadData();
  }
}
