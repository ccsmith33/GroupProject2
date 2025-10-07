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
  }

  render() {
    this.container.innerHTML = `
      <nav class="navbar navbar-expand-lg navbar-dark bg-primary">
        <div class="container">
          <a class="navbar-brand" href="#">Data Driven Studying</a>
          <div class="navbar-nav ms-auto">
            <a class="nav-link" href="#" id="loginBtn">Login</a>
          </div>
        </div>
      </nav>
    `;
  }

  setupEventListeners() {
    const loginBtn = document.getElementById("loginBtn");
    if (loginBtn) {
      loginBtn.addEventListener("click", (e) => {
        e.preventDefault();
        this.toggleLogin();
      });
    }
  }

  toggleLogin() {
    this.isLoggedIn = !this.isLoggedIn;
    const loginBtn = document.getElementById("loginBtn");
    
    if (loginBtn) {
      loginBtn.textContent = this.isLoggedIn ? "Logout" : "Login";
    }

    // Dispatch custom event for other components
    const event = new CustomEvent("authStateChanged", {
      detail: { isLoggedIn: this.isLoggedIn }
    });
    document.dispatchEvent(event);
  }

  getLoginState() {
    return this.isLoggedIn;
  }
}
