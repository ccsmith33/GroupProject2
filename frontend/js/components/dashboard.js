// Dashboard componentve
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
      <!-- Getting Started Guide -->
      <div class="getting-started-section">
        <h2 class="section-title">Getting Started</h2>
        <div class="getting-started-grid">
          <div class="getting-started-item">
            <div class="step-number">1</div>
            <h3 class="step-title">Upload Your Materials</h3>
            <p class="step-description">Start by uploading your study documents, notes, or textbooks.</p>
          </div>
          <div class="getting-started-item">
            <div class="step-number">2</div>
            <h3 class="step-title">AI Analysis</h3>
            <p class="step-description">Our AI will analyze your content and create personalized study guides.</p>
          </div>
          <div class="getting-started-item">
            <div class="step-number">3</div>
            <h3 class="step-title">Study & Track</h3>
            <p class="step-description">Access your study materials and track your progress over time.</p>
          </div>
        </div>
      </div>

      <!-- Section Divider -->
      <div class="section-divider"></div>

      <!-- Quick Actions & Stats -->
      <div class="quick-actions-section">
        <div class="quick-actions-stats-grid">
          <!-- Quick Actions -->
          <div class="quick-actions-grid">
            <div class="quick-action-card" onclick="document.getElementById('uploadTab').click()">
              <div class="action-icon">📤</div>
              <h3 class="action-title">Upload Documents</h3>
              <p class="action-description">Upload your study materials for AI analysis and personalized feedback</p>
              <div class="action-button">
                <span>Start Upload</span>
                <div class="btn-shine"></div>
              </div>
            </div>
            
            <div class="quick-action-card" onclick="document.getElementById('profileTab').click()">
              <div class="action-icon">📊</div>
              <h3 class="action-title">View Progress</h3>
              <p class="action-description">Check your study history and progress</p>
              <div class="action-button">
                <span>View Profile</span>
                <div class="btn-shine"></div>
              </div>
            </div>
            
            <div class="quick-action-card" onclick="document.getElementById('studyTab').click()">
              <div class="action-icon">🎓</div>
              <h3 class="action-title">Study Sessions</h3>
              <p class="action-description">Access your study materials and sessions</p>
              <div class="action-button">
                <span>Start Studying</span>
                <div class="btn-shine"></div>
              </div>
            </div>
          </div>
          
          <!-- User Stats -->
          <div class="stats-grid" id="userStatsSection">
            <div class="stat-card">
              <div class="stat-icon">📚</div>
              <div class="stat-number" id="totalFiles">0</div>
              <div class="stat-label">Files Uploaded</div>
            </div>
            <div class="stat-card">
              <div class="stat-icon">🎯</div>
              <div class="stat-number" id="studySessions">45</div>
              <div class="stat-label">Study Sessions</div>
            </div>
            <div class="stat-card">
              <div class="stat-icon">⭐</div>
              <div class="stat-number" id="averageScore">87</div>
              <div class="stat-label">Avg Score</div>
            </div>
            <div class="stat-card">
              <div class="stat-icon">🏆</div>
              <div class="stat-number" id="achievements">8</div>
              <div class="stat-label">Achievements</div>
            </div>
          </div>
        </div>
      </div>

      <!-- Section Divider -->
      <div class="section-divider"></div>

      <!-- Recent Activity -->
      <div class="recent-activity-section">
        <h2 class="section-title">Recent Activity</h2>
        <div class="activity-card" id="recentActivity">
          <div class="activity-list">
            <div class="activity-item">
              <div class="activity-icon">📄</div>
              <div class="activity-content">
                <h4 class="activity-title">Uploaded Math notes</h4>
                <p class="activity-time">2 hours ago</p>
              </div>
              <div class="activity-status upload">Uploaded</div>
            </div>
            
            <div class="activity-item">
              <div class="activity-icon">🧪</div>
              <div class="activity-content">
                <h4 class="activity-title">Completed Science quiz</h4>
                <p class="activity-time">5 hours ago</p>
              </div>
              <div class="activity-status completed">Completed</div>
            </div>
            
            <div class="activity-item">
              <div class="activity-icon">🏆</div>
              <div class="activity-content">
                <h4 class="activity-title">Earned "Quick Learner" badge</h4>
                <p class="activity-time">1 day ago</p>
              </div>
              <div class="activity-status achievement">Achievement</div>
            </div>
            
            <div class="activity-item">
              <div class="activity-icon">🤖</div>
              <div class="activity-content">
                <h4 class="activity-title">AI generated study guide for History</h4>
                <p class="activity-time">2 days ago</p>
              </div>
              <div class="activity-status generated">Generated</div>
            </div>
          </div>
        </div>
      </div>
    `;
  }

  async loadData() {
    try {
      await this.loadUserStats();
      await this.loadRecentActivity();
    } catch (error) {
      console.error('Error loading dashboard data:', error);
      this.showError('Failed to load dashboard data');
    }
  }

  async loadUserStats() {
    try {
      // Mock data for now - replace with actual API calls
      const stats = {
        totalFiles: 12,
        studySessions: 45,
        averageScore: 87,
        achievements: 8
      };

      document.getElementById('totalFiles').textContent = stats.totalFiles;
      document.getElementById('studySessions').textContent = stats.studySessions;
      document.getElementById('averageScore').textContent = stats.averageScore;
      document.getElementById('achievements').textContent = stats.achievements;
    } catch (error) {
      console.error('Error loading user stats:', error);
    }
  }

  async loadRecentActivity() {
    try {
      // Mock recent activity data
      const activities = [
        { type: 'upload', message: 'Uploaded Math notes', time: '2 hours ago', icon: '📄' },
        { type: 'study', message: 'Completed Science quiz', time: '5 hours ago', icon: '🧪' },
        { type: 'achievement', message: 'Earned "Quick Learner" badge', time: '1 day ago', icon: '🏆' },
        { type: 'analysis', message: 'AI generated study guide for History', time: '2 days ago', icon: '🤖' }
      ];

      const activityContainer = document.getElementById('recentActivity');
      if (activities.length === 0) {
        activityContainer.innerHTML = '<div class="text-center text-muted">No recent activity</div>';
        return;
      }

      activityContainer.innerHTML = activities.map(activity => `
        <div class="activity-item">
          <div class="activity-icon">${activity.icon}</div>
          <div class="activity-content">
            <div class="activity-message">${activity.message}</div>
            <div class="activity-time">${activity.time}</div>
          </div>
        </div>
      `).join('');
    } catch (error) {
      console.error('Error loading recent activity:', error);
      document.getElementById('recentActivity').innerHTML = 
        '<div class="text-center text-danger">Failed to load recent activity</div>';
    }
  }

  renderStats() {
    // This method can be called to refresh the stats display
    this.loadUserStats();
  }

  renderRecentActivity() {
    // This method can be called to refresh the recent activity display
    this.loadRecentActivity();
  }

  formatTimestamp(timestamp) {
    const now = new Date();
    const time = new Date(timestamp);
    const diffInMinutes = Math.floor((now - time) / (1000 * 60));
    
    if (diffInMinutes < 1) return 'Just now';
    if (diffInMinutes < 60) return `${diffInMinutes} minutes ago`;
    
    const diffInHours = Math.floor(diffInMinutes / 60);
    if (diffInHours < 24) return `${diffInHours} hours ago`;
    
    const diffInDays = Math.floor(diffInHours / 24);
    return `${diffInDays} days ago`;
  }

  switchToTab(tabName) {
    // This method can be used to programmatically switch tabs
    const tabElement = document.getElementById(tabName + 'Tab');
    if (tabElement) {
      tabElement.click();
    }
  }

  showError(message) {
    // Show error message to user
    console.error('Dashboard Error:', message);
    // You can implement a toast notification or alert here
  }

  updateWithResults(results) {
    console.log('Dashboard received results:', results);
    // Refresh stats and recent activity when new results come in
    this.loadUserStats();
    this.loadRecentActivity();
  }

  updateProgress() {
    // Update progress indicators
    this.loadUserStats();
  }
}

export default Dashboard;

