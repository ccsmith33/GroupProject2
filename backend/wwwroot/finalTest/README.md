# Student Study AI - Frontend Application

A comprehensive single-page application (SPA) built with vanilla JavaScript, HTML5, and CSS3 that provides an AI-powered study platform for students.

## 🚀 Features

### Core Functionality

- **File Management**: Upload, organize, and manage study materials with drag-and-drop support
- **AI-Powered Analysis**: Automatic subject detection and content analysis using OpenAI GPT models
- **Study Tools**: Generate study guides, interactive quizzes, and AI chat assistance
- **Progress Tracking**: Comprehensive analytics with charts and knowledge level monitoring
- **User Authentication**: Guest mode and registered user accounts with different AI model access

### Key Components

#### 1. File Manager (`js/components/fileManager.js`)

- Drag-and-drop file upload with validation
- Auto-grouping by subject with AI detection
- File processing status tracking
- Soft delete functionality
- Custom subject group management

#### 2. Study Tools (`js/components/studyTools.js`)

- Study guide generation from uploaded materials
- Interactive quiz creation and taking
- AI chat interface with conversation history
- Model selection (GPT-4o for authenticated users, GPT-4o-mini for guests)

#### 3. Dashboard (`js/components/dashboard.js`)

- Overview statistics and quick actions
- Recent activity feed
- Learning progress indicators
- Quick access to all features

#### 4. Progress Analytics (`js/components/progressAnalytics.js`)

- Interactive charts using Chart.js
- Subject-specific progress tracking
- Achievement system
- Data export capabilities (CSV, JSON)

#### 5. Onboarding Wizard (`js/components/wizard.js`)

- 5-step guided setup process
- File upload during onboarding
- Study tool preview
- Guest mode introduction

## 🏗️ Architecture

### File Structure

```
finalTest/
├── index.html              # Main HTML file
├── css/
│   ├── main.css           # Core styles and variables
│   ├── components.css     # Component-specific styles
│   ├── wizard.css         # Onboarding wizard styles
│   └── responsive.css     # Mobile-first responsive design
├── js/
│   ├── app.js             # Main application controller
│   ├── config.js          # Configuration and constants
│   ├── state/
│   │   ├── store.js       # Global state management
│   │   └── auth-state.js  # Authentication state
│   ├── api/
│   │   ├── auth.js        # Authentication API calls
│   │   ├── files.js       # File management API
│   │   ├── analysis.js    # AI analysis API
│   │   └── knowledge.js   # Knowledge tracking API
│   ├── components/
│   │   ├── wizard.js      # Onboarding wizard
│   │   ├── fileManager.js # File management component
│   │   ├── studyTools.js  # Study tools component
│   │   ├── dashboard.js   # Dashboard component
│   │   └── progressAnalytics.js # Analytics component
│   └── utils/
│       ├── notifications.js # Toast notification system
│       ├── validation.js    # Form validation utilities
│       └── helpers.js       # General utility functions
└── README.md              # This file
```

### State Management

The application uses a custom state management system (`store.js`) that provides:

- Centralized state storage
- Reactive updates
- Middleware support
- Local storage persistence

### API Integration

All API calls are abstracted into dedicated modules:

- **Authentication**: User registration, login, logout
- **Files**: Upload, processing, grouping, deletion
- **Analysis**: Study guide generation, quiz creation, AI chat
- **Knowledge**: Progress tracking, analytics, preferences

## 🎨 Design System

### Color Palette

- **Primary**: #2C5AA0 (Blue)
- **Accent**: #4A90E2 (Light Blue)
- **Success**: #28A745 (Green)
- **Warning**: #FFC107 (Yellow)
- **Danger**: #DC3545 (Red)
- **Info**: #17A2B8 (Cyan)

### Typography

- **Font Family**: System fonts (Inter, -apple-system, BlinkMacSystemFont, etc.)
- **Font Sizes**: Responsive scale from 0.75rem to 3rem
- **Font Weights**: 400 (normal), 500 (medium), 600 (semibold), 700 (bold)

### Spacing

- **Base Unit**: 0.25rem (4px)
- **Scale**: 0.25rem, 0.5rem, 0.75rem, 1rem, 1.5rem, 2rem, 3rem, 4rem, 6rem

### Components

- **Cards**: Rounded corners, subtle shadows, hover effects
- **Buttons**: Multiple variants (primary, secondary, outline, etc.)
- **Forms**: Consistent styling with validation states
- **Modals**: Overlay system with smooth animations
- **Notifications**: Toast system with different types

## 📱 Responsive Design

The application is built with a mobile-first approach:

- **Mobile**: < 768px
- **Tablet**: 768px - 1024px
- **Desktop**: > 1024px

Key responsive features:

- Flexible grid system
- Collapsible navigation
- Touch-friendly interactions
- Optimized typography scaling
- Adaptive component layouts

## 🔧 Configuration

### Environment Variables

The application uses a configuration system (`config.js`) that includes:

- API base URL
- AI model selection
- File size limits
- UI timing constants
- Feature flags

### Guest Mode

- Limited to 5 file uploads
- Uses GPT-4o-mini model
- Data stored in localStorage
- Prompts to create account

### Authenticated Mode

- Unlimited file uploads
- Uses GPT-4o model
- Data persisted on server
- Full feature access

## 🚀 Getting Started

### Prerequisites

- Modern web browser (Chrome, Firefox, Safari, Edge)
- Backend API running on localhost:5000
- Internet connection for AI model access

### Installation

1. Ensure the backend is running
2. Open `index.html` in a web browser
3. The application will automatically initialize

### Development

1. Make changes to JavaScript/CSS files
2. Refresh the browser to see changes
3. Use browser developer tools for debugging

## 🧪 Testing

### Manual Testing

1. **File Upload**: Test drag-and-drop and file selection
2. **Authentication**: Test guest mode and user registration
3. **Study Tools**: Generate study guides and quizzes
4. **Analytics**: View progress charts and export data
5. **Responsive**: Test on different screen sizes

### Browser Compatibility

- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

## 🔒 Security

### Data Protection

- JWT token-based authentication
- Secure API communication
- Input validation and sanitization
- XSS protection through proper escaping

### Privacy

- No data collection without consent
- Guest mode for anonymous usage
- Clear data retention policies
- GDPR compliance considerations

## 📈 Performance

### Optimization Strategies

- Lazy loading of components
- Efficient state management
- Minimal DOM manipulation
- Optimized asset loading
- Responsive image handling

### Metrics

- First Contentful Paint: < 1.5s
- Largest Contentful Paint: < 2.5s
- Cumulative Layout Shift: < 0.1
- First Input Delay: < 100ms

## 🐛 Troubleshooting

### Common Issues

1. **CORS Errors**: Ensure backend is running and CORS is configured
2. **File Upload Fails**: Check file size limits and supported formats
3. **AI Features Not Working**: Verify API key is valid and has credits
4. **Charts Not Displaying**: Ensure Chart.js is loaded correctly

### Debug Mode

Enable debug mode by setting `localStorage.setItem('debug', 'true')` in browser console.

## 🤝 Contributing

### Code Style

- Use ES6+ features
- Follow consistent naming conventions
- Add comments for complex logic
- Maintain responsive design principles

### Pull Request Process

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## 📄 License

This project is part of the Student Study AI platform. All rights reserved.

## 🆘 Support

For support and questions:

1. Check the troubleshooting section
2. Review the browser console for errors
3. Ensure all dependencies are loaded
4. Verify backend API connectivity

## 🔮 Future Enhancements

### Planned Features

- Offline mode support
- Advanced quiz types
- Collaborative study groups
- Mobile app version
- Integration with learning management systems

### Technical Improvements

- Service worker implementation
- Web Components migration
- TypeScript conversion
- Automated testing suite
- Performance monitoring

---

**Note**: This frontend application requires the Student Study AI backend API to function properly. Ensure the backend is running and accessible before using the application.
