class LandingPage {
  constructor(container) {
    if (!container) {
      console.error("LandingPage: Container is null or undefined");
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
      <div class="landing-container">
        <!-- Hero Section -->
        <section class="hero-section">
          <div class="hero-content">

            <div class="hero-main">
              <div class="hero-text">
                <h1 class="hero-title">
                  <span class="title-main">Empowering Every Student,</span>
                  <span class="title-accent">Every Study.</span>
                </h1>
                <p class="hero-subtitle">
                  Transform your study sessions with AI-powered insights, personalized learning paths, 
                  and engaging tools that make education fun and effective. Join thousands of students 
                  who are already studying smarter with StudySpree.
                </p>
                <div class="hero-buttons">
                  <button class="btn btn-primary" id="getStartedBtn">
                    <span>Get Started</span>
                    <div class="btn-shine"></div>
                  </button>
                  <button class="btn btn-secondary" id="learnMoreBtn">
                    <span>Learn More</span>
                  </button>
                </div>
              </div>
              <div class="hero-visual">
                <div class="robot-animation-container">
                  <!-- Robot Animation Area -->
                  <div class="animation-stage">
                    <!-- Robot Character -->
                    <div class="robot-character">
                      <div class="robot-body">🤖</div>
                      <div class="robot-eyes">
                        <div class="eye left-eye">👁️</div>
                        <div class="eye right-eye">👁️</div>
                      </div>
                      <div class="vacuum-hose"></div>
                    </div>
                    
                    <!-- Messy Notes (Before) -->
                    <div class="messy-notes">
                      <div class="note note-1">📚</div>
                      <div class="note note-2">📖</div>
                      <div class="note note-3">📄</div>
                      <div class="note note-4">📝</div>
                      <div class="note note-5">📋</div>
                      <div class="note note-6">📑</div>
                      <div class="note note-7">📚</div>
                      <div class="note note-8">📖</div>
                      <div class="note note-9">📄</div>
                      <div class="note note-10">📝</div>
                    </div>
                    
                    <!-- Vacuum Effect -->
                    <div class="vacuum-effect">
                      <!-- Particles removed for cleaner animation -->
                    </div>
                    
                    <!-- Clean Notes (After) -->
                    <div class="clean-notes">
                      <div class="clean-note clean-note-1">📝</div>
                    </div>
                    
                    <!-- Processing Animation -->
                    <div class="processing-animation">
                      <div class="processing-text">Processing...</div>
                      <div class="processing-dots">
                        <span class="dot">.</span>
                        <span class="dot">.</span>
                        <span class="dot">.</span>
                      </div>
                    </div>
                    
                    <!-- Success Message -->
                    <div class="success-message">
                      <div class="success-icon">✨</div>
                      <div class="success-text">Notes Organized!</div>
                    </div>
                  </div>
                  
                  <!-- Animation Description -->
                  <div class="animation-description">
                    <h3 class="description-title">AI-Powered Study Organization</h3>
                    <p class="description-text">
                      Watch our intelligent robot transform your messy notes into perfectly organized study materials!
                    </p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </section>

        <!-- How It Works Section -->
        <section class="how-it-works-section">
          <div class="how-it-works-content">
            <h2 class="section-title">How It Works</h2>
            <div class="steps-grid">
              <div class="step-card" data-aos="fade-up" data-aos-delay="100">
                <div class="step-number">1</div>
                <div class="step-icon">📤</div>
                <h3>Upload Your Materials</h3>
                <p>Simply drag and drop your study materials - documents, notes, or any learning content. Our AI analyzes everything in seconds.</p>
              </div>
              <div class="step-card" data-aos="fade-up" data-aos-delay="200">
                <div class="step-number">2</div>
                <div class="step-icon">🤖</div>
                <h3>AI Analysis & Insights</h3>
                <p>Our advanced AI creates personalized study guides, identifies knowledge gaps, and suggests the best learning path for you.</p>
              </div>
              <div class="step-card" data-aos="fade-up" data-aos-delay="300">
                <div class="step-number">3</div>
                <div class="step-icon">🎯</div>
                <h3>Achieve Your Goals</h3>
                <p>Study smarter, not harder. Track your progress, maintain streaks, and reach your academic goals faster with personalized recommendations.</p>
              </div>
            </div>
          </div>
        </section>

        <!-- Features Section -->
        <section class="features-section">
          <div class="features-content">
            <h2 class="section-title">Why Students Love StudySpree</h2>
            <div class="features-grid">
              <div class="feature-card" data-aos="fade-up" data-aos-delay="100">
                <div class="feature-icon">🤖</div>
                <h3>Smart Study Guide Generator</h3>
                <p>AI-powered study guides that adapt to your learning style, pace, and preferences. Transform any material into personalized, engaging content that actually sticks!</p>
              </div>
              <div class="feature-card" data-aos="fade-up" data-aos-delay="200">
                <div class="feature-icon">💡</div>
                <h3>Interactive Study Tools</h3>
                <p>Create smart notes, generate flashcards, and access AI-powered quizzes. Everything you need for effective studying in one place.</p>
              </div>
              <div class="feature-card" data-aos="fade-up" data-aos-delay="300">
                <div class="feature-icon">🎯</div>
                <h3>Goal Setter & Streak Counter</h3>
                <p>Build consistent study habits with daily streaks, goal setting, and achievement badges. Stay motivated with gamified learning experiences!</p>
              </div>
              <div class="feature-card" data-aos="fade-up" data-aos-delay="400">
                <div class="feature-icon">📊</div>
                <h3>Personalized Learning Insights</h3>
                <p>Beautiful analytics and progress tracking that shows exactly where you excel and where you need focus. Data-driven insights for smarter studying.</p>
              </div>
            </div>
          </div>
        </section>

        <!-- Testimonials Section -->
        <section class="testimonials-section">
          <div class="testimonials-content">
            <h2 class="section-title">Student Success Stories</h2>
            <div class="testimonials-grid">
              <div class="testimonial-card" data-aos="fade-right">
                <div class="testimonial-content">
                  <p>"StudySpree completely transformed how I study! My grades improved by 45% and I actually enjoy learning now. The AI insights are incredible!"</p>
                  <div class="testimonial-author">
                    <div class="author-avatar">👩‍🎓</div>
                    <div class="author-info">
                      <h4>Sarah Chen</h4>
                      <span>Computer Science, MIT</span>
                    </div>
                  </div>
                </div>
              </div>
              <div class="testimonial-card" data-aos="fade-left">
                <div class="testimonial-content">
                  <p>"The personalized study guides are amazing! I went from struggling with organic chemistry to acing my exams. The streak counter keeps me motivated!"</p>
                  <div class="testimonial-author">
                    <div class="author-avatar">👨‍🎓</div>
                    <div class="author-info">
                      <h4>Marcus Johnson</h4>
                      <span>Biology, Stanford</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </section>

        <!-- CTA Section -->
        <section class="cta-section">
          <div class="cta-content">
            <h2 class="cta-title">Ready to Start Your StudySpree?</h2>
            <p class="cta-subtitle">Join thousands of students who are already studying smarter with StudySpree.</p>
            <div class="cta-buttons">
              <button class="btn btn-primary btn-large" id="ctaLearnMoreBtn">
                <span>Get Started</span>
                <div class="btn-shine"></div>
              </button>
              <button class="btn btn-primary btn-large" id="ctaGetStartedBtn">
                <span>Start Your Journey</span>
                <div class="btn-shine"></div>
              </button>
            </div>
          </div>
        </section>

        <!-- Product Overview Modal -->
        <div class="modal fade" id="productModal" tabindex="-1" aria-labelledby="productModalLabel" aria-hidden="true">
          <div class="modal-dialog modal-lg">
            <div class="modal-content product-content">
              <div class="modal-header">
                <h1 class="modal-title" id="productModalLabel">StudySpree Product</h1>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
              </div>
              <div class="modal-body">
                <div class="product-content">
                  <p class="product-intro">StudySpree is more than a study app — it's your personalized productivity hub.</p>
                  
                  <h2>Here's what you get:</h2>
                  
                  <div class="product-features">
                    <div class="feature-item">
                      <div class="feature-icon">🪶</div>
                      <div class="feature-text">
                        <h3>Smart Study Spaces</h3>
                        <p>Create and customize digital study rooms for different subjects.</p>
                      </div>
                    </div>
                    
                    <div class="feature-item">
                      <div class="feature-icon">✨</div>
                      <div class="feature-text">
                        <h3>Collaboration Tools</h3>
                        <p>Chat, share resources, and join group study sessions with friends.</p>
                      </div>
                    </div>
                    
                    <div class="feature-item">
                      <div class="feature-icon">📅</div>
                      <div class="feature-text">
                        <h3>Planner & Reminders</h3>
                        <p>Keep deadlines and exams organized with smart notifications.</p>
                      </div>
                    </div>
                    
                    <div class="feature-item">
                      <div class="feature-icon">📊</div>
                      <div class="feature-text">
                        <h3>Progress Tracker</h3>
                        <p>Visualize your learning habits and celebrate milestones.</p>
                      </div>
                    </div>
                    
                    <div class="feature-item">
                      <div class="feature-icon">☁️</div>
                      <div class="feature-text">
                        <h3>Cloud Sync</h3>
                        <p>Access your study materials anywhere, anytime.</p>
                      </div>
                    </div>
                  </div>
                  
                  <p class="product-summary">Everything you need — streamlined, synced, and styled for your workflow.</p>
                </div>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-primary" data-bs-dismiss="modal">Got It</button>
              </div>
            </div>
          </div>
        </div>

        <!-- Features Modal -->
        <div class="modal fade" id="featuresModal" tabindex="-1" aria-labelledby="featuresModalLabel" aria-hidden="true">
          <div class="modal-dialog modal-xl">
            <div class="modal-content features-content">
              <div class="modal-header">
                <h1 class="modal-title" id="featuresModalLabel">StudySpree Features</h1>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
              </div>
              <div class="modal-body">
                <div class="features-content">
                  <p class="features-intro">Explore StudySpree's best features, designed to make learning feel good.</p>
                  
                  <div class="features-table">
                    <table class="features-table-content">
                      <thead>
                        <tr>
                          <th>Feature</th>
                          <th>What It Does</th>
                          <th>Why It's Awesome</th>
                        </tr>
                      </thead>
                      <tbody>
                        <tr>
                          <td><span class="feature-emoji">🧠</span> Smart Recommendations</td>
                          <td>Suggests study guides, flashcards, or topics based on what you're studying.</td>
                          <td>Personalized for your goals.</td>
                        </tr>
                        <tr>
                          <td><span class="feature-emoji">🎨</span> Theme Customizer</td>
                          <td>Choose from moody pastels, soft gradients, or sky-inspired palettes.</td>
                          <td>Make studying aesthetic.</td>
                        </tr>
                        <tr>
                          <td><span class="feature-emoji">🤝</span> Study Circles</td>
                          <td>Connect with peers who share your subjects or goals.</td>
                          <td>Learn together, grow together.</td>
                        </tr>
                        <tr>
                          <td><span class="feature-emoji">⏱️</span> Focus Mode</td>
                          <td>Block distractions and time your sessions.</td>
                          <td>Boost productivity instantly.</td>
                        </tr>
                        <tr>
                          <td><span class="feature-emoji">📈</span> Stats Dashboard</td>
                          <td>Track time spent, consistency, and improvement trends.</td>
                          <td>Stay motivated with visual progress.</td>
                        </tr>
                      </tbody>
                    </table>
                  </div>
                </div>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-primary" data-bs-dismiss="modal">Awesome!</button>
              </div>
            </div>
          </div>
        </div>

        <!-- Pricing Modal -->
        <div class="modal fade" id="pricingModal" tabindex="-1" aria-labelledby="pricingModalLabel" aria-hidden="true">
          <div class="modal-dialog modal-lg">
            <div class="modal-content pricing-content">
              <div class="modal-header">
                <h1 class="modal-title" id="pricingModalLabel">StudySpree Pricing</h1>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
              </div>
              <div class="modal-body">
                <div class="pricing-content">
                  <p class="pricing-intro">StudySpree keeps it simple: Fair, flexible, and student-friendly.</p>
                  
                  <div class="pricing-table">
                    <table class="pricing-table-content">
                      <thead>
                        <tr>
                          <th>Plan</th>
                          <th>Price</th>
                          <th>Best For</th>
                          <th>Includes</th>
                        </tr>
                      </thead>
                      <tbody>
                        <tr>
                          <td><span class="plan-emoji">🎒</span> Free Plan</td>
                          <td>$0</td>
                          <td>Every student</td>
                          <td>Core features, limited storage</td>
                        </tr>
                        <tr>
                          <td><span class="plan-emoji">🌱</span> Pro Plan</td>
                          <td>$4.99/mo</td>
                          <td>Active learners</td>
                          <td>Unlimited spaces, premium themes, progress analytics</td>
                        </tr>
                        <tr>
                          <td><span class="plan-emoji">🚀</span> Team Plan</td>
                          <td>$9.99/mo</td>
                          <td>Study groups & clubs</td>
                          <td>Shared spaces, group tracking, admin controls</td>
                        </tr>
                      </tbody>
                    </table>
                  </div>
                  
                  <div class="pricing-note">
                    <p>💡 <strong>Students always get a 20% discount</strong> — because education should be accessible.</p>
                  </div>
                </div>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-primary" data-bs-dismiss="modal">Choose Plan</button>
              </div>
            </div>
          </div>
        </div>

        <!-- Status Modal -->
        <div class="modal fade" id="statusModal" tabindex="-1" aria-labelledby="statusModalLabel" aria-hidden="true">
          <div class="modal-dialog modal-lg">
            <div class="modal-content status-content">
              <div class="modal-header">
                <h1 class="modal-title" id="statusModalLabel">🟢 Status</h1>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
              </div>
              <div class="modal-body">
                <div class="status-content">
                  <p class="status-intro">All systems go!</p>
                  <p>Check StudySpree's live system updates here:</p>
                  
                  <div class="status-services">
                    <div class="status-item">
                      <span class="status-indicator">🟢</span>
                      <span class="status-service">Study Spaces: Operational</span>
                    </div>
                    <div class="status-item">
                      <span class="status-indicator">🟢</span>
                      <span class="status-service">Login & Accounts: Operational</span>
                    </div>
                    <div class="status-item">
                      <span class="status-indicator">🟢</span>
                      <span class="status-service">Sync Services: Stable</span>
                    </div>
                    <div class="status-item">
                      <span class="status-indicator">🟢</span>
                      <span class="status-service">Analytics Dashboard: Stable</span>
                    </div>
                  </div>
                  
                  <p class="status-note">If we ever go down, we'll post real-time updates here so you're never left guessing.</p>
                </div>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-primary" data-bs-dismiss="modal">Great!</button>
              </div>
            </div>
          </div>
        </div>

        <!-- Contact Modal -->
        <div class="modal fade" id="contactModal" tabindex="-1" aria-labelledby="contactModalLabel" aria-hidden="true">
          <div class="modal-dialog modal-lg">
            <div class="modal-content contact-content">
              <div class="modal-header">
                <h1 class="modal-title" id="contactModalLabel">📞 Contact</h1>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
              </div>
              <div class="modal-body">
                <div class="contact-content">
                  <p class="contact-intro">We'd love to hear from you!</p>
                  <p>Whether you're sharing feedback, partnership ideas, or just want to say hi — we're here.</p>
                  
                  <div class="contact-info">
                    <div class="contact-item">
                      <h3>📧 Email:</h3>
                      <p><a href="mailto:hello@studyspree.com">hello@studyspree.com</a></p>
                    </div>
                    
                    <div class="contact-item">
                      <h3>📍 Address:</h3>
                      <p>StudySpree HQ, University of Alabama, Tuscaloosa, AL</p>
                    </div>
                  </div>
                </div>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-primary" data-bs-dismiss="modal">Got It</button>
              </div>
            </div>
          </div>
        </div>

        <!-- Help Center Modal -->
        <div class="modal fade" id="helpCenterModal" tabindex="-1" aria-labelledby="helpCenterModalLabel" aria-hidden="true">
          <div class="modal-dialog modal-lg">
            <div class="modal-content help-center-content">
              <div class="modal-header">
                <h1 class="modal-title" id="helpCenterModalLabel">💬 Help Center</h1>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
              </div>
              <div class="modal-body">
                <div class="help-center-content">
                  <p class="help-intro">Your first stop for answers.</p>
                  <p>Explore step-by-step guides, FAQs, and video tutorials to get the most out of StudySpree.</p>
                  
                  <h2>🔍 Search topics like:</h2>
                  
                  <div class="help-topics">
                    <div class="topic-item">
                      <span class="topic-bullet">•</span>
                      <span class="topic-text">Setting up study rooms</span>
                    </div>
                    <div class="topic-item">
                      <span class="topic-bullet">•</span>
                      <span class="topic-text">Customizing your dashboard</span>
                    </div>
                    <div class="topic-item">
                      <span class="topic-bullet">•</span>
                      <span class="topic-text">Using focus timers</span>
                    </div>
                    <div class="topic-item">
                      <span class="topic-bullet">•</span>
                      <span class="topic-text">Sharing notes with friends</span>
                    </div>
                  </div>
                  
                  <div class="help-link">
                    <p>📘 <strong>Visit:</strong> <a href="https://help.studyspree.com" target="_blank">help.studyspree.com</a></p>
                  </div>
                </div>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-primary" data-bs-dismiss="modal">Visit Help Center</button>
              </div>
            </div>
          </div>
        </div>

        <!-- Terms of Service Modal -->
        <div class="modal fade" id="termsServiceModal" tabindex="-1" aria-labelledby="termsServiceModalLabel" aria-hidden="true">
          <div class="modal-dialog modal-lg">
            <div class="modal-content terms-service-content">
              <div class="modal-header">
                <h1 class="modal-title" id="termsServiceModalLabel">StudySpree Terms of Service</h1>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
              </div>
              <div class="modal-body">
                <div class="terms-service-content">
                  <p class="effective-date"><strong>Effective Date: October 9, 2025</strong></p>
                  
                  <p>Welcome to StudySpree! These Terms of Service ("Terms") govern your use of our website, tools, and services.</p>
                  <p>By accessing or using StudySpree, you agree to these Terms. Please read them carefully.</p>
                  
                  <h2>1. Acceptance of Terms</h2>
                  <p>By using StudySpree, you agree to comply with these Terms and our <a href="#" data-bs-toggle="modal" data-bs-target="#privacyPolicyModal" data-bs-dismiss="modal">Privacy Policy</a>.</p>
                  <p>If you disagree with any part, please do not use the website.</p>
                  
                  <h2>2. Use of Our Service</h2>
                  <p>You agree to use StudySpree for lawful purposes only.</p>
                  <p>You must not:</p>
                  <ul>
                    <li>Copy, modify, or distribute StudySpree content without permission.</li>
                    <li>Attempt to hack, disrupt, or misuse the platform.</li>
                    <li>Impersonate other users or share false information.</li>
                  </ul>
                  <p>We reserve the right to suspend or terminate accounts that violate these rules.</p>
                  
                  <h2>3. Account Responsibilities</h2>
                  <p>You are responsible for:</p>
                  <ul>
                    <li>Maintaining the confidentiality of your login credentials.</li>
                    <li>All activity under your account.</li>
                    <li>Notifying us immediately of unauthorized access.</li>
                  </ul>
                  
                  <h2>4. Intellectual Property</h2>
                  <p>All StudySpree content — including designs, logos, text, and code — is owned or licensed by StudySpree.</p>
                  <p>You may not use any of our trademarks or branding without written permission.</p>
                  
                  <h2>5. User-Generated Content</h2>
                  <p>StudySpree may allow you to upload posts, notes, or comments.</p>
                  <p>By submitting content, you grant StudySpree a non-exclusive, royalty-free license to display and share it within the platform.</p>
                  <p>You retain ownership of your content, but you're responsible for what you post.</p>
                  
                  <h2>6. Disclaimer of Warranties</h2>
                  <p>StudySpree is provided "as is."</p>
                  <p>We do not guarantee uninterrupted access, error-free performance, or that all information will always be accurate.</p>
                  <p>Use StudySpree at your own discretion — though we do our best to keep things running smoothly!</p>
                  
                  <h2>7. Limitation of Liability</h2>
                  <p>To the maximum extent permitted by law, StudySpree and its team are not liable for any indirect, incidental, or consequential damages resulting from your use of the platform.</p>
                  
                  <h2>8. Termination</h2>
                  <p>We may suspend or terminate access to your account at any time, for any reason, including violation of these Terms.</p>
                  <p>You may also delete your account at any time by contacting <a href="mailto:support@studyspree.com">support@studyspree.com</a>.</p>
                  
                  <h2>9. Changes to These Terms</h2>
                  <p>We may update these Terms as our platform evolves. Continued use after updates means you accept the new Terms.</p>
                  
                  <h2>10. Contact Us</h2>
                  <p>For questions or feedback about these Terms, reach out to:</p>
                  <p>📧 <a href="mailto:legal@studyspree.com">legal@studyspree.com</a></p>
                </div>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-primary" data-bs-dismiss="modal">I Agree</button>
              </div>
            </div>
          </div>
        </div>

        <!-- Privacy Policy Modal -->
        <div class="modal fade" id="privacyPolicyModal" tabindex="-1" aria-labelledby="privacyPolicyModalLabel" aria-hidden="true">
          <div class="modal-dialog modal-lg">
            <div class="modal-content privacy-policy-content">
              <div class="modal-header">
                <h1 class="modal-title" id="privacyPolicyModalLabel">StudySpree Privacy Policy</h1>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
              </div>
              <div class="modal-body">
                <div class="privacy-policy-content">
                  <p class="effective-date"><strong>Effective Date: October 9, 2025</strong></p>
                  
                  <p>Welcome to StudySpree — where learning meets creativity!</p>
                  <p>We care deeply about your privacy and are committed to protecting your personal information. This Privacy Policy explains what data we collect, how we use it, and your rights as a StudySpree user.</p>
                  
                  <h2>1. Information We Collect</h2>
                  <p>We collect information to help make your experience better:</p>
                  
                  <h3>Personal Information</h3>
                  <p>Name, email address, login credentials, or other details you share when signing up.</p>
                  
                  <h3>Usage Data</h3>
                  <p>Pages visited, features used, time spent on the site, and device/browser type.</p>
                  
                  <h3>Cookies & Tracking Data</h3>
                  <p>Used to personalize your experience and improve performance (see our <a href="#" data-bs-toggle="modal" data-bs-target="#cookiesPolicyModal" data-bs-dismiss="modal">Cookie Policy</a> for details).</p>
                  
                  <h2>2. How We Use Your Information</h2>
                  <p>We use your information to:</p>
                  <ul>
                    <li>Provide and maintain our services.</li>
                    <li>Personalize your study tools, dashboard, and saved preferences.</li>
                    <li>Communicate updates, support messages, and service announcements.</li>
                    <li>Analyze site performance and improve features.</li>
                    <li>Ensure security and prevent fraud.</li>
                  </ul>
                  <p><strong>We do not sell or rent your data to anyone.</strong></p>
                  
                  <h2>3. How We Share Your Information</h2>
                  <p>We only share data when necessary, and always securely:</p>
                  
                  <h3>Service Providers</h3>
                  <p>Trusted partners who help operate the website (e.g., hosting, analytics, email).</p>
                  
                  <h3>Legal Requirements</h3>
                  <p>If required by law or to protect our rights.</p>
                  
                  <h3>With Your Consent</h3>
                  <p>When you explicitly allow us to share specific information.</p>
                  
                  <h2>4. Data Retention</h2>
                  <p>We retain your data as long as your account is active or as needed to provide our services. You can request deletion anytime by contacting <a href="mailto:support@studyspree.com">support@studyspree.com</a>.</p>
                  
                  <h2>5. Your Rights</h2>
                  <p>You have full control over your personal data. You can:</p>
                  <ul>
                    <li>Access, update, or delete your account information.</li>
                    <li>Opt out of marketing communications.</li>
                    <li>Request data portability or restriction of processing.</li>
                  </ul>
                  <p>To exercise any of these rights, email <a href="mailto:privacy@studyspree.com">privacy@studyspree.com</a>.</p>
                  
                  <h2>6. Data Security</h2>
                  <p>We use industry-standard security measures (encryption, secure servers, etc.) to keep your information safe.</p>
                  <p>While no system is 100% secure, we continuously improve our protections to keep StudySpree trustworthy.</p>
                  
                  <h2>7. Children's Privacy</h2>
                  <p>StudySpree is intended for users 13 years and older. We do not knowingly collect information from children under 13.</p>
                  
                  <h2>8. Changes to This Policy</h2>
                  <p>We may update this Privacy Policy as our features evolve. The updated date will always appear at the top.</p>
                  
                  <h2>9. Contact Us</h2>
                  <p>Questions? We'd love to help.</p>
                  <p>📧 <a href="mailto:privacy@studyspree.com">privacy@studyspree.com</a></p>
                </div>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-primary" data-bs-dismiss="modal">I Understand</button>
              </div>
            </div>
          </div>
        </div>

        <!-- Cookies Policy Modal -->
        <div class="modal fade" id="cookiesPolicyModal" tabindex="-1" aria-labelledby="cookiesPolicyModalLabel" aria-hidden="true">
          <div class="modal-dialog modal-lg">
            <div class="modal-content cookies-policy-content">
              <div class="modal-header">
                <h1 class="modal-title" id="cookiesPolicyModalLabel">StudySpree Cookie Policy</h1>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
              </div>
              <div class="modal-body">
                <div class="cookies-policy-content">
                  <p class="effective-date"><strong>Effective Date: October 9, 2025</strong></p>
                  
                  <p>At StudySpree, we want your study experience to be smooth, personalized, and secure. To make that possible, we use cookies and similar technologies on our website. This page explains what cookies are, how we use them, and your choices regarding them.</p>
                  
                  <h2>What Are Cookies?</h2>
                  <p>Cookies are small text files stored on your device when you visit a website. They help us remember your preferences, improve your experience, and understand how you interact with StudySpree.</p>
                  
                  <h2>How We Use Cookies</h2>
                  <p>We use cookies for several purposes, including:</p>
                  
                  <h3>Essential Cookies</h3>
                  <ul>
                    <li>Necessary for the website to function properly.</li>
                    <li><strong>Examples:</strong> Keeping you logged in, saving session information.</li>
                  </ul>
                  
                  <h3>Performance & Analytics Cookies</h3>
                  <ul>
                    <li>Help us understand how visitors use StudySpree.</li>
                    <li>We track anonymous data to improve features and functionality.</li>
                  </ul>
                  
                  <h3>Functional Cookies</h3>
                  <ul>
                    <li>Remember your preferences, like theme colors or language settings.</li>
                    <li>Make your experience smoother and more personalized.</li>
                  </ul>
                  
                  <h3>Advertising & Marketing Cookies (optional, if applicable)</h3>
                  <ul>
                    <li>Help us show relevant content and promotions.</li>
                    <li>We do not share personally identifiable information with advertisers.</li>
                  </ul>
                  
                  <h2>Your Cookie Choices</h2>
                  <p>Most browsers allow you to manage cookies. You can:</p>
                  <ul>
                    <li>Accept or decline cookies.</li>
                    <li>Delete cookies that have already been set.</li>
                  </ul>
                  
                  <div class="warning-note">
                    <p>⚠️ <strong>Note:</strong> Disabling essential cookies may affect the functionality of StudySpree.</p>
                  </div>
                  
                  <h2>Third-Party Cookies</h2>
                  <p>Some features on StudySpree may involve trusted third-party services (like analytics or social media tools). These services may also use cookies. We do not control these cookies, and their use is subject to the third party's privacy policy.</p>
                  
                  <h2>Updates to This Cookie Policy</h2>
                  <p>We may update this policy from time to time to reflect changes in technology, regulations, or website features. Check this page periodically for updates.</p>
                  
                  <h2>Contact Us</h2>
                  <p>If you have any questions or concerns about our use of cookies, please reach out to us:</p>
                  <p><strong>Email:</strong> <a href="mailto:support@studyspree.com">support@studyspree.com</a></p>
                </div>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-primary" data-bs-dismiss="modal">I Understand</button>
              </div>
            </div>
          </div>
        </div>

        <!-- Footer -->
        <footer class="footer">
          <div class="footer-content">
            <div class="footer-brand">
              <h3>StudySpree</h3>
              <p>Empowering Every Student, Every Study.</p>
            </div>
            <div class="footer-links">
              <div class="footer-column">
                <h4>Product</h4>
                <a href="#" data-bs-toggle="modal" data-bs-target="#productModal">Product Overview</a>
                <a href="#" data-bs-toggle="modal" data-bs-target="#featuresModal">Features</a>
                <a href="#" data-bs-toggle="modal" data-bs-target="#pricingModal">Pricing</a>
              </div>
              <div class="footer-column">
                <h4>Company</h4>
                <a href="#" onclick="document.dispatchEvent(new CustomEvent('tabChanged', {detail: {activeTab: 'about'}})); return false;">About</a>
                <a href="#" onclick="document.dispatchEvent(new CustomEvent('tabChanged', {detail: {activeTab: 'about'}})); setTimeout(() => {document.getElementById('team-section')?.scrollIntoView({behavior: 'smooth'});}, 100); return false;">Meet the Team</a>
              </div>
              <div class="footer-column">
                <h4>Support</h4>
                <a href="#" data-bs-toggle="modal" data-bs-target="#helpCenterModal">Help Center</a>
                <a href="#" data-bs-toggle="modal" data-bs-target="#contactModal">Contact</a>
                <a href="#" data-bs-toggle="modal" data-bs-target="#statusModal">Status</a>
              </div>
              <div class="footer-column">
                <h4>Legal</h4>
                <a href="#" data-bs-toggle="modal" data-bs-target="#privacyPolicyModal">Privacy Policy</a>
                <a href="#" data-bs-toggle="modal" data-bs-target="#termsServiceModal">Terms of Service</a>
                <a href="#" data-bs-toggle="modal" data-bs-target="#cookiesPolicyModal">Cookie Policy</a>
              </div>
            </div>
          </div>
          <div class="footer-bottom">
            <p>&copy; 2025 StudySpree. A student project by MIS321 Group 2. All rights reserved.</p>
          </div>
        </footer>
        
        <!-- Scroll to Top Button -->
        <button class="scroll-to-top" id="scrollToTopBtn" aria-label="Scroll to top">
          <i class="fas fa-arrow-up"></i>
        </button>
      </div>
    `;
  }

  setupEventListeners() {
    // Get Started Button
    const getStartedBtn = document.getElementById("getStartedBtn");
    if (getStartedBtn) {
      getStartedBtn.addEventListener("click", () => {
        this.emit("navigateToPage", "upload");
      });
    }

    // Learn More Button
    const learnMoreBtn = document.getElementById("learnMoreBtn");
    if (learnMoreBtn) {
      learnMoreBtn.addEventListener("click", () => {
        document.querySelector(".features-section").scrollIntoView({
          behavior: "smooth"
        });
      });
    }

    // Robot Animation Setup
    this.initRobotAnimation();

    // CTA Buttons
    const ctaGetStartedBtn = document.getElementById("ctaGetStartedBtn");
    if (ctaGetStartedBtn) {
      ctaGetStartedBtn.addEventListener("click", () => {
        this.emit("navigateToPage", "upload");
      });
    }

    const ctaLearnMoreBtn = document.getElementById("ctaLearnMoreBtn");
    if (ctaLearnMoreBtn) {
      ctaLearnMoreBtn.addEventListener("click", () => {
        document.querySelector(".features-section").scrollIntoView({
          behavior: "smooth"
        });
      });
    }

    // Theme Toggle (if implemented)
    const themeToggle = document.querySelector(".theme-toggle");
    if (themeToggle) {
      themeToggle.addEventListener("click", () => {
        document.body.classList.toggle("dark-mode");
        localStorage.setItem("theme", document.body.classList.contains("dark-mode") ? "dark" : "light");
      });
    }

    // Scroll to Top Button
    const scrollToTopBtn = document.getElementById("scrollToTopBtn");
    if (scrollToTopBtn) {
      scrollToTopBtn.addEventListener("click", () => {
        window.scrollTo({ top: 0, behavior: "smooth" });
      });
    }

    // Navbar scroll behavior
    window.addEventListener("scroll", () => {
      const navbar = document.querySelector(".studyspree-navbar");
      const scrollToTopBtn = document.getElementById("scrollToTopBtn");
      
      if (navbar) {
        if (window.scrollY > 50) {
          navbar.classList.add("scrolled");
        } else {
          navbar.classList.remove("scrolled");
        }
      }
      
      if (scrollToTopBtn) {
        if (window.scrollY > 300) {
          scrollToTopBtn.classList.add("visible");
        } else {
          scrollToTopBtn.classList.remove("visible");
        }
      }
    });
  }

  startAnimations() {
    // Add animation classes after a short delay
    setTimeout(() => {
      const floatingElements = document.querySelectorAll(".floating-icon");
      floatingElements.forEach((element, index) => {
        element.style.animationDelay = `${index * 0.5}s`;
        element.classList.add("animate");
      });

      // Progress bar animation
      const progressFill = document.querySelector(".progress-fill");
      if (progressFill) {
        setTimeout(() => {
          progressFill.style.width = "75%";
        }, 1000);
      }
    }, 500);
  }

  initRobotAnimation() {
    // Start the robot animation loop
    this.startRobotAnimation();
  }

  startRobotAnimation() {
    // Animation sequence timing
    setTimeout(() => {
      this.showMessyNotes();
    }, 500);

    setTimeout(() => {
      this.startVacuumAnimation();
    }, 2000);

    setTimeout(() => {
      this.showProcessing();
    }, 4000);

    setTimeout(() => {
      this.showCleanNotes();
    }, 6000);

    setTimeout(() => {
      this.showSuccess();
    }, 8000);

    // Loop the animation every 12 seconds
    setTimeout(() => {
      this.resetAnimation();
      this.startRobotAnimation();
    }, 12000);
  }

  showMessyNotes() {
    const notes = document.querySelectorAll('.note');
    notes.forEach((note, index) => {
      setTimeout(() => {
        note.style.opacity = '1';
        note.style.transform = 'translateY(0) scale(1)';
        note.style.animation = 'noteScatter 0.5s ease-out';
      }, index * 100);
    });
  }

  startVacuumAnimation() {
    const robot = document.querySelector('.robot-character');
    const hose = document.querySelector('.vacuum-hose');
    
    // Robot moves toward notes
    if (robot) {
      robot.style.animation = 'robotMove 2s ease-in-out';
    }

    // Vacuum hose extends
    if (hose) {
      hose.style.animation = 'hoseExtend 1s ease-out 0.5s both';
    }

    // Notes get sucked in
    const notes = document.querySelectorAll('.note');
    notes.forEach((note, index) => {
      setTimeout(() => {
        note.style.animation = 'noteSuction 1s ease-in forwards';
      }, 1000 + (index * 100));
    });
  }

  showProcessing() {
    const processing = document.querySelector('.processing-animation');
    if (processing) {
      processing.style.display = 'block';
      processing.style.animation = 'processingPulse 2s ease-in-out infinite';
    }
  }

  showCleanNotes() {
    const processing = document.querySelector('.processing-animation');
    if (processing) {
      processing.style.display = 'none';
    }

    const cleanNotes = document.querySelectorAll('.clean-note');
    cleanNotes.forEach((note, index) => {
      setTimeout(() => {
        note.style.opacity = '1';
        note.style.transform = 'translateY(0) scale(1)';
        note.style.animation = 'noteAppear 0.5s ease-out';
      }, index * 200);
    });
  }

  showSuccess() {
    const success = document.querySelector('.success-message');
    if (success) {
      success.style.display = 'block';
      success.style.animation = 'successBounce 1s ease-out';
    }
  }

  resetAnimation() {
    // Reset all elements to initial state
    const notes = document.querySelectorAll('.note');
    const cleanNotes = document.querySelectorAll('.clean-note');
    const processing = document.querySelector('.processing-animation');
    const success = document.querySelector('.success-message');
    const robot = document.querySelector('.robot-character');
    const hose = document.querySelector('.vacuum-hose');

    // Reset messy notes
    notes.forEach(note => {
      note.style.opacity = '0';
      note.style.transform = 'translateY(20px) scale(0.8)';
      note.style.animation = 'none';
    });

    // Reset clean notes
    cleanNotes.forEach(note => {
      note.style.opacity = '0';
      note.style.transform = 'translateY(20px) scale(0.8)';
      note.style.animation = 'none';
    });

    // Hide processing and success
    if (processing) processing.style.display = 'none';
    if (success) success.style.display = 'none';

    // Reset robot
    if (robot) robot.style.animation = 'none';
    if (hose) hose.style.animation = 'none';
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
