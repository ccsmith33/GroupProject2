// Dashboard component
class Dashboard {
  constructor(container) {
    this.container = container;
    this.studySessions = [];
    this.recentFiles = [];
    this.init();
  }

  init() {
    this.render();
    this.loadData();
  }

  render() {
    this.container.innerHTML = `
      <div class="row">
        <div class="col-12">
          <h1>Welcome to Data Driven Studying</h1>
          <p class="lead">Upload your study materials and get personalized AI feedback</p>
        </div>
      </div>

      <!-- Quick Actions -->
      <div class="row mt-4">
        <div class="col-md-4">
          <div class="card text-center">
            <div class="card-body">
              <i class="fas fa-upload fa-3x text-primary mb-3"></i>
              <h5 class="card-title">Upload Documents</h5>
              <p class="card-text">Upload your study materials for AI analysis</p>
              <button class="btn btn-primary" onclick="document.getElementById('uploadTab').click()">Start Upload</button>
            </div>
          </div>
        </div>
        <div class="col-md-4">
          <div class="card text-center">
            <div class="card-body">
              <i class="fas fa-chart-line fa-3x text-success mb-3"></i>
              <h5 class="card-title">View Progress</h5>
              <p class="card-text">Check your study history and progress</p>
              <button class="btn btn-success" onclick="document.getElementById('profileTab').click()">View Profile</button>
            </div>
          </div>
        </div>
        <div class="col-md-4">
          <div class="card text-center">
            <div class="card-body">
              <i class="fas fa-graduation-cap fa-3x text-info mb-3"></i>
              <h5 class="card-title">Study Sessions</h5>
              <p class="card-text">Access your study materials and sessions</p>
              <button class="btn btn-info" onclick="document.getElementById('studyTab').click()">Start Studying</button>
            </div>
          </div>
        </div>
      </div>
    `;
  }

  async loadData() {
    // Dashboard no longer needs to load study data
    // This is now handled by the Profile component
  }

  // Simplified dashboard - just shows welcome and quick actions
  updateWithResults(results) {
    // Results are now handled by Profile component
    console.log("Results received by dashboard, forwarding to profile");
  }

  updateProgress() {
    // Progress updates are now handled by Profile component
  }
}
