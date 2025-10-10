// File handling utilities

import { validationManager } from "./validation.js";
import { filesAPI } from "../api/files.js";
import { logger } from "./logger.js";
import { CONFIG } from "../config.js";

class FileHandler {
  // Validate file
  static validateFile(file) {
    if (!file) {
      return {
        isValid: false,
        message: "No file provided",
      };
    }

    // Use validation manager for file validation
    const validation = validationManager.validateFile(file);

    if (!validation.isValid) {
      return validation;
    }

    // Additional checks
    const errors = [];

    // Check file size
    if (file.size > CONFIG.MAX_FILE_SIZE) {
      errors.push(
        `File size exceeds maximum of ${CONFIG.formatFileSize(
          CONFIG.MAX_FILE_SIZE
        )}`
      );
    }

    // Check file type
    if (!CONFIG.ALLOWED_FILE_TYPES.includes(file.type)) {
      errors.push(`File type "${file.type}" is not supported`);
    }

    // Check file name
    if (file.name.length > 255) {
      errors.push("File name is too long (max 255 characters)");
    }

    return {
      isValid: errors.length === 0,
      message: errors[0] || "",
      errors: errors,
    };
  }

  // Process file for upload
  static async processFile(file, userId, subject = "", level = "") {
    try {
      // Validate file first
      const validation = this.validateFile(file);
      if (!validation.isValid) {
        throw new Error(validation.message);
      }

      // Log file processing
      logger.info("Processing file:", {
        name: file.name,
        size: file.size,
        type: file.type,
      });

      // Upload file using Files API
      const result = await filesAPI.uploadFile(file, userId, subject, level);

      logger.info("File processed successfully:", result);

      return result;
    } catch (error) {
      logger.error("File processing error:", error);
      throw error;
    }
  }

  // Chunk large file for upload
  static chunkFile(file, chunkSize = 1024 * 1024) {
    // Default chunk size: 1MB
    const chunks = [];
    let offset = 0;

    while (offset < file.size) {
      const chunk = file.slice(offset, offset + chunkSize);
      chunks.push({
        data: chunk,
        index: chunks.length,
        offset: offset,
        size: chunk.size,
        total: file.size,
      });
      offset += chunkSize;
    }

    logger.info(`File chunked into ${chunks.length} pieces`);
    return chunks;
  }

  // Upload file with progress tracking
  static async uploadFileWithProgress(
    file,
    userId,
    subject,
    level,
    onProgress
  ) {
    try {
      // Validate file
      const validation = this.validateFile(file);
      if (!validation.isValid) {
        throw new Error(validation.message);
      }

      // For files larger than 5MB, use chunked upload
      if (file.size > 5 * 1024 * 1024) {
        return await this.uploadFileChunked(
          file,
          userId,
          subject,
          level,
          onProgress
        );
      }

      // For smaller files, use regular upload
      const result = await filesAPI.uploadFile(file, userId, subject, level);

      if (onProgress) {
        onProgress(100);
      }

      return result;
    } catch (error) {
      logger.error("File upload error:", error);
      throw error;
    }
  }

  // Upload file in chunks with progress
  static async uploadFileChunked(file, userId, subject, level, onProgress) {
    const chunks = this.chunkFile(file);
    const uploadedChunks = [];

    for (let i = 0; i < chunks.length; i++) {
      const chunk = chunks[i];

      // Create a new File object for the chunk
      const chunkFile = new File([chunk.data], `${file.name}.part${i}`, {
        type: file.type,
      });

      // Upload chunk
      const result = await filesAPI.uploadFile(
        chunkFile,
        userId,
        subject,
        level
      );
      uploadedChunks.push(result);

      // Update progress
      if (onProgress) {
        const progress = Math.round(((i + 1) / chunks.length) * 100);
        onProgress(progress);
      }
    }

    logger.info("All chunks uploaded successfully");
    return uploadedChunks[uploadedChunks.length - 1]; // Return last chunk result
  }

  // Get file extension
  static getFileExtension(filename) {
    const parts = filename.split(".");
    return parts.length > 1 ? parts.pop().toLowerCase() : "";
  }

  // Get file name without extension
  static getFileNameWithoutExtension(filename) {
    const parts = filename.split(".");
    if (parts.length > 1) {
      parts.pop();
    }
    return parts.join(".");
  }

  // Check if file is image
  static isImage(file) {
    return file.type.startsWith("image/");
  }

  // Check if file is document
  static isDocument(file) {
    const documentTypes = [
      "application/pdf",
      "application/msword",
      "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
      "application/vnd.ms-powerpoint",
      "application/vnd.openxmlformats-officedocument.presentationml.presentation",
      "text/plain",
    ];
    return documentTypes.includes(file.type);
  }

  // Read file as text
  static readFileAsText(file) {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = (e) => resolve(e.target.result);
      reader.onerror = (e) => reject(new Error("Failed to read file"));
      reader.readAsText(file);
    });
  }

  // Read file as data URL
  static readFileAsDataURL(file) {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = (e) => resolve(e.target.result);
      reader.onerror = (e) => reject(new Error("Failed to read file"));
      reader.readAsDataURL(file);
    });
  }

  // Read file as array buffer
  static readFileAsArrayBuffer(file) {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = (e) => resolve(e.target.result);
      reader.onerror = (e) => reject(new Error("Failed to read file"));
      reader.readAsArrayBuffer(file);
    });
  }

  // Create file from blob
  static createFileFromBlob(blob, filename, type) {
    return new File([blob], filename, { type: type || blob.type });
  }

  // Download file from URL
  static async downloadFile(url, filename) {
    try {
      const response = await fetch(url);
      const blob = await response.blob();
      const link = document.createElement("a");
      link.href = URL.createObjectURL(blob);
      link.download = filename;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(link.href);
    } catch (error) {
      logger.error("File download error:", error);
      throw error;
    }
  }
}

export default FileHandler;
