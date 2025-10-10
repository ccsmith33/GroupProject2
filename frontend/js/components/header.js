// Header component for navigation
class Header {
  constructor(container) {
    this.container = container;
    this.isLoggedIn = false;
    this.init();
  }

  init() {
    this.render();
    this.setupEventListeners();
    this.setInitialActiveTab();
  }

  render() {
    this.container.innerHTML = `
      <nav class="navbar navbar-expand-lg navbar-light studyspree-navbar">
        <div class="container">
          <a class="navbar-brand" href="#" id="homeTab">StudySpree</a>
          
          <!-- Centralized Navigation Tabs -->
          <div class="navbar-nav mx-auto">
            <a class="nav-link" href="#" id="homeNavTab">Home</a>
            <a class="nav-link" href="#" id="aboutTab">About</a>
            <a class="nav-link" href="#" id="studyTab">Study Aids</a>
            <a class="nav-link" href="#" id="uploadTab">Upload Documents</a>
            <a class="nav-link" href="#" id="profileTab">Profile</a>
          </div>
          
          <!-- Auth Buttons -->
          <div class="navbar-nav">
            <a class="nav-link" href="#" id="loginBtn" data-bs-toggle="modal" data-bs-target="#loginModal">Login</a>
            <a class="nav-link" href="#" id="signupBtn" data-bs-toggle="modal" data-bs-target="#signupModal">Sign Up</a>
          </div>
        </div>
      </nav>
      
      <!-- Login Modal -->
      <div class="modal fade" id="loginModal" tabindex="-1" aria-labelledby="loginModalLabel" aria-hidden="true">
        <div class="modal-dialog">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title" id="loginModalLabel">Login</h5>
              <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body">
              <form id="loginForm">
                <div class="mb-3">
                  <label for="loginEmail" class="form-label">Email</label>
                  <input type="email" class="form-control" id="loginEmail" required>
                </div>
                <div class="mb-3">
                  <label for="loginPassword" class="form-label">Password</label>
                  <input type="password" class="form-control" id="loginPassword" required>
                </div>
                <div class="mb-3 form-check">
                  <input type="checkbox" class="form-check-input" id="rememberMe">
                  <label class="form-check-label" for="rememberMe">Remember me</label>
                </div>
              </form>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
              <button type="button" class="btn btn-primary" id="loginSubmitBtn">Login</button>
            </div>
          </div>
        </div>
      </div>

      <!-- Sign Up Modal -->
      <div class="modal fade" id="signupModal" tabindex="-1" aria-labelledby="signupModalLabel" aria-hidden="true">
        <div class="modal-dialog">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title" id="signupModalLabel">Sign Up</h5>
              <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body">
              <form id="signupForm">
                <div class="mb-3">
                  <label for="signupName" class="form-label">Full Name</label>
                  <input type="text" class="form-control" id="signupName" required>
                </div>
                <div class="mb-3">
                  <label for="signupEmail" class="form-label">Email</label>
                  <input type="email" class="form-control" id="signupEmail" required>
                </div>
                <div class="mb-3">
                  <label for="signupSchool" class="form-label">School</label>
                  <select class="form-select" id="signupSchool" required>
                    <option value="">Select your school</option>
                    <option value="harvard">Harvard University</option>
                    <option value="stanford">Stanford University</option>
                    <option value="mit">Massachusetts Institute of Technology</option>
                    <option value="yale">Yale University</option>
                    <option value="princeton">Princeton University</option>
                    <option value="columbia">Columbia University</option>
                    <option value="upenn">University of Pennsylvania</option>
                    <option value="caltech">California Institute of Technology</option>
                    <option value="dartmouth">Dartmouth College</option>
                    <option value="brown">Brown University</option>
                    <option value="cornell">Cornell University</option>
                    <option value="duke">Duke University</option>
                    <option value="northwestern">Northwestern University</option>
                    <option value="rice">Rice University</option>
                    <option value="vanderbilt">Vanderbilt University</option>
                    <option value="washington">Washington University in St. Louis</option>
                    <option value="emory">Emory University</option>
                    <option value="georgetown">Georgetown University</option>
                    <option value="carnegie">Carnegie Mellon University</option>
                    <option value="usc">University of Southern California</option>
                    <option value="ucla">University of California, Los Angeles</option>
                    <option value="berkeley">University of California, Berkeley</option>
                    <option value="michigan">University of Michigan</option>
                    <option value="virginia">University of Virginia</option>
                    <option value="unc">University of North Carolina at Chapel Hill</option>
                    <option value="wake">Wake Forest University</option>
                    <option value="tufts">Tufts University</option>
                    <option value="nyu">New York University</option>
                    <option value="boston">Boston University</option>
                    <option value="brandeis">Brandeis University</option>
                    <option value="case">Case Western Reserve University</option>
                    <option value="lehigh">Lehigh University</option>
                    <option value="northeastern">Northeastern University</option>
                    <option value="tulane">Tulane University</option>
                    <option value="georgia">Georgia Institute of Technology</option>
                    <option value="illinois">University of Illinois at Urbana-Champaign</option>
                    <option value="wisconsin">University of Wisconsin-Madison</option>
                    <option value="texas">University of Texas at Austin</option>
                    <option value="florida">University of Florida</option>
                    <option value="ohio">Ohio State University</option>
                    <option value="penn">Pennsylvania State University</option>
                    <option value="rutgers">Rutgers University</option>
                    <option value="maryland">University of Maryland</option>
                    <option value="minnesota">University of Minnesota</option>
                    <option value="purdue">Purdue University</option>
                    <option value="indiana">Indiana University</option>
                    <option value="iowa">University of Iowa</option>
                    <option value="arizona">University of Arizona</option>
                    <option value="arizona-state">Arizona State University</option>
                    <option value="colorado">University of Colorado Boulder</option>
                    <option value="utah">University of Utah</option>
                    <option value="oregon">University of Oregon</option>
                    <option value="washington-state">Washington State University</option>
                    <option value="other">Other</option>
                  </select>
                </div>
                <div class="mb-3">
                  <label for="signupPassword" class="form-label">Password</label>
                  <input type="password" class="form-control" id="signupPassword" required minlength="8">
                  <div class="form-text">Password must be at least 8 characters long</div>
                </div>
                <div class="mb-3">
                  <label for="signupConfirmPassword" class="form-label">Confirm Password</label>
                  <input type="password" class="form-control" id="signupConfirmPassword" required>
                </div>
                <div class="mb-3 form-check">
                  <input type="checkbox" class="form-check-input" id="agreeTerms" required>
                  <label class="form-check-label" for="agreeTerms">I agree to the Terms of Service and Privacy Policy</label>
                </div>
              </form>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
              <button type="button" class="btn btn-primary" id="signupSubmitBtn">Sign Up</button>
            </div>
          </div>
        </div>
      </div>
    `;
  }

  setupEventListeners() {
    // Navigation tab events
    const homeTab = document.getElementById("homeTab");
    if (homeTab) {
      homeTab.addEventListener("click", (e) => {
        e.preventDefault();
        this.handleTabClick("home");
      });
    }

    const homeNavTab = document.getElementById("homeNavTab");
    if (homeNavTab) {
      homeNavTab.addEventListener("click", (e) => {
        e.preventDefault();
        this.handleTabClick("home");
      });
    }

    const aboutTab = document.getElementById("aboutTab");
    if (aboutTab) {
      aboutTab.addEventListener("click", (e) => {
        e.preventDefault();
        this.handleTabClick("about");
      });
    }

    const studyTab = document.getElementById("studyTab");
    if (studyTab) {
      studyTab.addEventListener("click", (e) => {
        e.preventDefault();
        this.handleTabClick("study");
      });
    }

    const uploadTab = document.getElementById("uploadTab");
    if (uploadTab) {
      uploadTab.addEventListener("click", (e) => {
        e.preventDefault();
        this.handleTabClick("upload");
      });
    }

    const profileTab = document.getElementById("profileTab");
    if (profileTab) {
      profileTab.addEventListener("click", (e) => {
        e.preventDefault();
        this.handleTabClick("profile");
      });
    }

    // Login form submission
    const loginSubmitBtn = document.getElementById("loginSubmitBtn");
    if (loginSubmitBtn) {
      loginSubmitBtn.addEventListener("click", (e) => {
        e.preventDefault();
        this.handleLogin();
      });
    }

    // Signup form submission
    const signupSubmitBtn = document.getElementById("signupSubmitBtn");
    if (signupSubmitBtn) {
      signupSubmitBtn.addEventListener("click", (e) => {
        e.preventDefault();
        this.handleSignup();
      });
    }

    // Password confirmation validation
    const confirmPassword = document.getElementById("signupConfirmPassword");
    if (confirmPassword) {
      confirmPassword.addEventListener("input", () => {
        this.validatePasswordMatch();
      });
    }

    const signupPassword = document.getElementById("signupPassword");
    if (signupPassword) {
      signupPassword.addEventListener("input", () => {
        this.validatePasswordMatch();
      });
    }
  }

  handleTabClick(tabName) {
    // Remove active class from all tabs
    const allTabs = document.querySelectorAll('.nav-link[id$="Tab"]');
    allTabs.forEach(tab => {
      tab.classList.remove('active');
    });

    // Add active class to clicked tab
    const activeTab = document.getElementById(`${tabName}Tab`);
    if (activeTab) {
      activeTab.classList.add('active');
    }

    // Dispatch custom event for other components to handle tab switching
    const event = new CustomEvent("tabChanged", {
      detail: { activeTab: tabName }
    });
    document.dispatchEvent(event);

    console.log(`Switched to ${tabName} tab`);
  }

  // Set initial active tab
  setInitialActiveTab() {
    // Set Home tab as active by default
    const homeTab = document.getElementById("homeNavTab");
    if (homeTab) {
      homeTab.classList.add('active');
    }
  }

  async handleLogin() {
    const email = document.getElementById("loginEmail").value;
    const password = document.getElementById("loginPassword").value;
    const rememberMe = document.getElementById("rememberMe").checked;

    if (!email || !password) {
      this.showError("Please fill in all fields");
      return;
    }

    try {
      // Simulate API call - replace with actual API
      const response = await this.mockLogin(email, password);
      
      if (response.success) {
        this.isLoggedIn = true;
        this.updateNavbar();
        this.closeModal("loginModal");
        this.showSuccess("Login successful!");
        
        // Store user data
        if (rememberMe) {
          localStorage.setItem("userData", JSON.stringify(response.user));
        }
        sessionStorage.setItem("userData", JSON.stringify(response.user));
        
        // Dispatch event
        this.dispatchAuthEvent();
      } else {
        this.showError(response.message || "Login failed");
      }
    } catch (error) {
      console.error("Login error:", error);
      this.showError("Login failed. Please try again.");
    }
  }

  async handleSignup() {
    const name = document.getElementById("signupName").value;
    const email = document.getElementById("signupEmail").value;
    const school = document.getElementById("signupSchool").value;
    const password = document.getElementById("signupPassword").value;
    const confirmPassword = document.getElementById("signupConfirmPassword").value;
    const agreeTerms = document.getElementById("agreeTerms").checked;

    // Validation
    if (!name || !email || !school || !password || !confirmPassword) {
      this.showError("Please fill in all fields");
      return;
    }

    if (password !== confirmPassword) {
      this.showError("Passwords do not match");
      return;
    }

    if (password.length < 8) {
      this.showError("Password must be at least 8 characters long");
      return;
    }

    if (!agreeTerms) {
      this.showError("Please agree to the Terms of Service and Privacy Policy");
      return;
    }

    try {
      // Simulate API call - replace with actual API
      const response = await this.mockSignup({ name, email, school, password });
      
      if (response.success) {
        this.isLoggedIn = true;
        this.updateNavbar();
        this.closeModal("signupModal");
        this.showSuccess("Account created successfully!");
        
        // Store user data
        sessionStorage.setItem("userData", JSON.stringify(response.user));
        
        // Dispatch event
        this.dispatchAuthEvent();
      } else {
        this.showError(response.message || "Signup failed");
      }
    } catch (error) {
      console.error("Signup error:", error);
      this.showError("Signup failed. Please try again.");
    }
  }

  validatePasswordMatch() {
    const password = document.getElementById("signupPassword").value;
    const confirmPassword = document.getElementById("signupConfirmPassword").value;
    const confirmField = document.getElementById("signupConfirmPassword");
    
    if (confirmPassword && password !== confirmPassword) {
      confirmField.setCustomValidity("Passwords do not match");
    } else {
      confirmField.setCustomValidity("");
    }
  }

  updateNavbar() {
    const loginBtn = document.getElementById("loginBtn");
    const signupBtn = document.getElementById("signupBtn");
    
    if (this.isLoggedIn) {
      loginBtn.textContent = "Logout";
      loginBtn.onclick = (e) => {
        e.preventDefault();
        this.handleLogout();
      };
      signupBtn.style.display = "none";
    } else {
      loginBtn.textContent = "Login";
      loginBtn.setAttribute("data-bs-toggle", "modal");
      loginBtn.setAttribute("data-bs-target", "#loginModal");
      signupBtn.style.display = "block";
    }
  }

  handleLogout() {
    this.isLoggedIn = false;
    localStorage.removeItem("userData");
    sessionStorage.removeItem("userData");
    this.updateNavbar();
    this.showSuccess("Logged out successfully");
    this.dispatchAuthEvent();
  }

  closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
      const bootstrapModal = bootstrap.Modal.getInstance(modal);
      if (bootstrapModal) {
        bootstrapModal.hide();
      }
    }
  }

  showError(message) {
    // Create or update error message
    let errorDiv = document.getElementById("authError");
    if (!errorDiv) {
      errorDiv = document.createElement("div");
      errorDiv.id = "authError";
      errorDiv.className = "alert alert-danger mt-3";
      document.body.appendChild(errorDiv);
    }
    errorDiv.textContent = message;
    errorDiv.style.display = "block";
    
    setTimeout(() => {
      errorDiv.style.display = "none";
    }, 5000);
  }

  showSuccess(message) {
    // Create or update success message
    let successDiv = document.getElementById("authSuccess");
    if (!successDiv) {
      successDiv = document.createElement("div");
      successDiv.id = "authSuccess";
      successDiv.className = "alert alert-success mt-3";
      document.body.appendChild(successDiv);
    }
    successDiv.textContent = message;
    successDiv.style.display = "block";
    
    setTimeout(() => {
      successDiv.style.display = "none";
    }, 3000);
  }

  dispatchAuthEvent() {
    const event = new CustomEvent("authStateChanged", {
      detail: { isLoggedIn: this.isLoggedIn }
    });
    document.dispatchEvent(event);
  }

  // Mock API functions - replace with actual API calls
  async mockLogin(email, password) {
    // Simulate API delay
    await new Promise(resolve => setTimeout(resolve, 1000));
    
    // Mock validation
    if (email === "test@example.com" && password === "password123") {
      return {
        success: true,
        user: {
          id: 1,
          name: "Test User",
          email: email,
          school: "Harvard University"
        }
      };
    } else {
      return {
        success: false,
        message: "Invalid email or password"
      };
    }
  }

  async mockSignup(userData) {
    // Simulate API delay
    await new Promise(resolve => setTimeout(resolve, 1000));
    
    // Mock validation
    if (userData.email === "existing@example.com") {
      return {
        success: false,
        message: "Email already exists"
      };
    }
    
    return {
      success: true,
      user: {
        id: Math.floor(Math.random() * 1000),
        name: userData.name,
        email: userData.email,
        school: userData.school
      }
    };
  }

  getLoginState() {
    return this.isLoggedIn;
  }
}
