# API Contract - Student Study AI Platform

## Analysis Endpoint

### POST /api/analysis/analyze

**Request Body:**

```json
{
  "fileType": "pdf|image|text|powerpoint",
  "content": "string",
  "subject": "string",
  "studentLevel": "beginner|intermediate|advanced"
}
```

**Response:**

```json
{
  "analysisId": "uuid",
  "timestamp": "2024-01-01T00:00:00Z",
  "overallScore": 85,
  "feedback": {
    "summary": "string",
    "strengths": ["string"],
    "weakAreas": ["string"],
    "recommendations": ["string"]
  },
  "detailedAnalysis": {
    "conceptUnderstanding": 80,
    "problemSolving": 75,
    "completeness": 90,
    "accuracy": 85
  },
  "studyPlan": {
    "priorityTopics": ["string"],
    "suggestedResources": ["string"],
    "estimatedTime": "2-3 hours",
    "nextSteps": ["string"]
  },
  "confidence": 0.85
}
```

## File Upload Endpoint

### POST /api/files/upload

**Request:** Multipart form data

- `file`: File object
- `subject`: string
- `studentLevel`: string

**Response:**

```json
{
  "fileId": "uuid",
  "fileName": "string",
  "fileType": "string",
  "fileSize": 1024,
  "uploadedAt": "2024-01-01T00:00:00Z",
  "status": "uploaded|processing|ready|error"
}
```

## Error Responses

**400 Bad Request:**

```json
{
  "error": "Invalid file type",
  "message": "Only PDF, images, and text files are supported"
}
```

**500 Internal Server Error:**

```json
{
  "error": "Analysis failed",
  "message": "Unable to process the uploaded content"
}
```
