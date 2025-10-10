class AboutPage {
  constructor(container) {
    if (!container) {
      console.error("AboutPage: Container is null or undefined");
      return;
    }
    this.container = container;
    this.eventListeners = {};
    this.init();
  }

  init() {
    this.render();
    this.setupEventListeners();
    this.startAnimations();
  }

  render() {
    this.container.innerHTML = `
      <div class="about-container">
        <!-- Hero Section -->
        <section class="about-hero">
          <div class="about-hero-content">
            <h1 class="about-title">About StudySpree</h1>
            <p class="about-subtitle">
              We're on a mission to make studying fun, personalized, and motivating 
              through smart data and engaging visuals.
            </p>
          </div>
        </section>

        <!-- Mission Section -->
        <section class="mission-section">
          <div class="mission-content">
            <div class="mission-grid">
              <div class="mission-text">
                <h2 class="section-title">Our Mission</h2>
                <p class="mission-description">
                  StudySpree was born from a simple observation: traditional studying methods 
                  aren't working for everyone. We believe that every student deserves a 
                  personalized learning experience that adapts to their unique style and pace.
                </p>
                <p class="mission-description">
                  By combining cutting-edge AI technology with beautiful, engaging interfaces, 
                  we're transforming the way students learn, making education more accessible, 
                  effective, and enjoyable.
                </p>
                <div class="mission-stats">
                  <div class="stat-item">
                    <div class="stat-number">10K+</div>
                    <div class="stat-label">Happy Students</div>
                  </div>
                  <div class="stat-item">
                    <div class="stat-number">95%</div>
                    <div class="stat-label">Improved Grades</div>
                  </div>
                  <div class="stat-item">
                    <div class="stat-number">50+</div>
                    <div class="stat-label">Universities</div>
                  </div>
                </div>
              </div>
              <div class="mission-visual">
                <div class="mission-image">
                  <div class="mission-graphic">
                    <div class="graphic-title">Empowering Every Student</div>
                    <div class="graphic-elements">
                      <div class="learning-path">
                        <div class="path-dot path-start"></div>
                        <div class="path-line"></div>
                        <div class="path-dot path-middle"></div>
                        <div class="path-line"></div>
                        <div class="path-dot path-end"></div>
                      </div>
                      <div class="success-indicators">
                        <div class="indicator indicator-1">📈</div>
                        <div class="indicator indicator-2">🎯</div>
                        <div class="indicator indicator-3">⚡</div>
                      </div>
                    </div>
                    <div class="graphic-subtitle">From Challenge to Success</div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </section>

        <!-- Our Vision Section -->
        <section class="vision-section">
          <div class="vision-content">
            <div class="vision-grid">
              <div class="vision-visual">
                <div class="vision-image">
                  <div class="vision-graphic">
                    <div class="graphic-title">The Future of Learning</div>
                    <div class="vision-elements">
                      <div class="global-reach">
                        <div class="globe-icon">🌍</div>
                        <div class="reach-label">Global Access</div>
                      </div>
                      <div class="ai-brain">
                        <div class="brain-icon">🧠</div>
                        <div class="ai-label">AI-Powered</div>
                      </div>
                      <div class="personalization">
                        <div class="personal-icon">🎯</div>
                        <div class="personal-label">Personalized</div>
                      </div>
                    </div>
                    <div class="vision-subtitle">Every Student, Everywhere</div>
                  </div>
                </div>
              </div>
              <div class="vision-text">
                <h2 class="section-title">Our Vision</h2>
                <p class="vision-description">
                  We envision a world where every student has access to personalized, 
                  engaging, and effective learning tools. Where studying becomes a 
                  journey of discovery rather than a chore.
                </p>
                <p class="vision-description">
                  Our goal is to democratize education by making advanced AI-powered 
                  learning accessible to students everywhere, regardless of their 
                  background or learning style.
                </p>
                <div class="vision-values">
                  <div class="value-item">
                    <div class="value-icon">🎯</div>
                    <div class="value-text">
                      <h4>Personalized Learning</h4>
                      <p>Every student learns differently, and we adapt to that.</p>
                    </div>
                  </div>
                  <div class="value-item">
                    <div class="value-icon">🚀</div>
                    <div class="value-text">
                      <h4>Innovation First</h4>
                      <p>We're always pushing the boundaries of educational technology.</p>
                    </div>
                  </div>
                  <div class="value-item">
                    <div class="value-icon">🤝</div>
                    <div class="value-text">
                      <h4>Student Success</h4>
                      <p>Your success is our success. We're here to help you achieve your goals.</p>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </section>

        <!-- Team Section -->
        <section class="team-section" id="team-section">
          <div class="team-content">
            <h2 class="section-title">Meet the Team</h2>
            <p class="team-subtitle">
              Made by students, for students. A passionate group of developers and designers working 
              together to revolutionize how students learn.
            </p>
            <div class="team-grid">
              <div class="team-member" data-aos="zoom-in" data-aos-delay="100">
                <div class="member-avatar">👨‍💻</div>
                <h3>Clayton Smith</h3>
                <p class="member-role">Lead Developer</p>
                <p class="member-bio">Full-stack developer passionate about creating innovative learning solutions.</p>
              </div>
              <div class="team-member" data-aos="zoom-in" data-aos-delay="200">
                <div class="member-avatar">👨‍💻</div>
                <h3>Conner Vardeman</h3>
                <p class="member-role">Frontend Developer</p>
                <p class="member-bio">UI/UX enthusiast focused on creating beautiful and intuitive user experiences.</p>
              </div>
              <div class="team-member" data-aos="zoom-in" data-aos-delay="300">
                <div class="member-avatar">👨‍💻</div>
                <h3>Zach Zombron</h3>
                <p class="member-role">Backend Developer</p>
                <p class="member-bio">System architecture expert with a passion for scalable and efficient solutions.</p>
              </div>
              <div class="team-member" data-aos="zoom-in" data-aos-delay="400">
                <div class="member-avatar">👩‍💻</div>
                <h3>Preeti Mainali</h3>
                <p class="member-role">Project Manager</p>
                <p class="member-bio">Coordinating our mission to make studying more effective and engaging for students everywhere.</p>
              </div>
            </div>
          </div>
        </section>

        <!-- CTA Section -->
        <section class="about-cta">
          <div class="about-cta-content">
            <h2 class="cta-title">Ready to Join Our Mission?</h2>
            <p class="cta-subtitle">Start your personalized learning journey with StudySpree today.</p>
            <div class="cta-buttons">
              <button class="btn btn-primary btn-large" id="aboutGetStartedBtn">
                <span>Get Started Free</span>
                <div class="btn-shine"></div>
              </button>
              <button class="btn btn-primary btn-large" id="aboutLearnMoreBtn">
                <span>Contact Us</span>
                <div class="btn-shine"></div>
              </button>
            </div>
          </div>
        </section>
      </div>
    `;
  }

  setupEventListeners() {
    // Get Started Button
    const aboutGetStartedBtn = document.getElementById("aboutGetStartedBtn");
    if (aboutGetStartedBtn) {
      aboutGetStartedBtn.addEventListener("click", () => {
        this.emit("navigateToPage", "upload");
      });
    }

    // Contact Button
    const aboutLearnMoreBtn = document.getElementById("aboutLearnMoreBtn");
    if (aboutLearnMoreBtn) {
      aboutLearnMoreBtn.addEventListener("click", () => {
        // TODO: Implement contact modal or redirect to contact page
        showNotification("Contact feature coming soon!", "info");
      });
    }
  }

  startAnimations() {
    // Add animation classes after a short delay
    setTimeout(() => {
      const floatingStudents = document.querySelectorAll(".student-avatar");
      floatingStudents.forEach((student, index) => {
        student.style.animationDelay = `${index * 0.5}s`;
        student.classList.add("animate");
      });

      const studyElements = document.querySelectorAll(".study-book, .study-laptop, .study-lightbulb");
      studyElements.forEach((element, index) => {
        element.style.animationDelay = `${index * 0.3}s`;
        element.classList.add("animate");
      });
    }, 500);
  }

  on(event, callback) {
    if (!this.eventListeners[event]) {
      this.eventListeners[event] = [];
    }
    this.eventListeners[event].push(callback);
  }

  emit(event, data) {
    if (this.eventListeners[event]) {
      this.eventListeners[event].forEach(callback => callback(data));
    }
  }
}
