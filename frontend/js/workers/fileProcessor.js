// File Processing Web Worker
// Handles file processing in a separate thread to avoid blocking the UI

self.addEventListener("message", async (event) => {
  const { type, data } = event.data;

  try {
    switch (type) {
      case "processFile":
        await processFile(data);
        break;

      case "chunkFile":
        await chunkFile(data);
        break;

      case "validateFile":
        await validateFile(data);
        break;

      case "calculateHash":
        await calculateHash(data);
        break;

      default:
        throw new Error(`Unknown message type: ${type}`);
    }
  } catch (error) {
    self.postMessage({
      type: "error",
      error: {
        message: error.message,
        stack: error.stack,
      },
    });
  }
});

// Process file
async function processFile(data) {
  const { file, options } = data;

  self.postMessage({
    type: "progress",
    progress: 0,
    message: "Starting file processing...",
  });

  try {
    // Read file
    const arrayBuffer = await file.arrayBuffer();

    self.postMessage({
      type: "progress",
      progress: 30,
      message: "File read successfully",
    });

    // Process file based on type
    let processedData = arrayBuffer;

    if (options.compress) {
      self.postMessage({
        type: "progress",
        progress: 50,
        message: "Compressing file...",
      });
      // TODO: Implement compression
    }

    if (options.encrypt) {
      self.postMessage({
        type: "progress",
        progress: 70,
        message: "Encrypting file...",
      });
      // TODO: Implement encryption
    }

    self.postMessage({
      type: "progress",
      progress: 100,
      message: "File processing complete",
    });

    self.postMessage({
      type: "complete",
      data: {
        size: processedData.byteLength,
        processed: true,
      },
    });
  } catch (error) {
    throw new Error(`File processing failed: ${error.message}`);
  }
}

// Chunk file
async function chunkFile(data) {
  const { file, chunkSize } = data;
  const chunks = [];
  let offset = 0;
  const totalChunks = Math.ceil(file.size / chunkSize);

  self.postMessage({
    type: "progress",
    progress: 0,
    message: `Creating ${totalChunks} chunks...`,
  });

  while (offset < file.size) {
    const end = Math.min(offset + chunkSize, file.size);
    const chunk = file.slice(offset, end);
    const arrayBuffer = await chunk.arrayBuffer();

    chunks.push({
      index: chunks.length,
      offset: offset,
      size: arrayBuffer.byteLength,
      data: arrayBuffer,
      totalSize: file.size,
      totalChunks: totalChunks,
    });

    offset = end;

    // Report progress
    const progress = Math.round((chunks.length / totalChunks) * 100);
    self.postMessage({
      type: "progress",
      progress: progress,
      message: `Created chunk ${chunks.length}/${totalChunks}`,
    });
  }

  self.postMessage({
    type: "complete",
    data: {
      chunks: chunks,
      totalChunks: chunks.length,
    },
  });
}

// Validate file
async function validateFile(data) {
  const { file, maxSize, allowedTypes } = data;

  const errors = [];

  // Check file size
  if (file.size > maxSize) {
    errors.push(
      `File size (${formatFileSize(
        file.size
      )}) exceeds maximum (${formatFileSize(maxSize)})`
    );
  }

  // Check file type
  if (allowedTypes && !allowedTypes.includes(file.type)) {
    errors.push(`File type "${file.type}" is not allowed`);
  }

  // Check file name
  if (file.name.length > 255) {
    errors.push("File name is too long (max 255 characters)");
  }

  self.postMessage({
    type: "complete",
    data: {
      isValid: errors.length === 0,
      errors: errors,
    },
  });
}

// Calculate file hash
async function calculateHash(data) {
  const { file } = data;

  self.postMessage({
    type: "progress",
    progress: 0,
    message: "Reading file...",
  });

  const arrayBuffer = await file.arrayBuffer();

  self.postMessage({
    type: "progress",
    progress: 50,
    message: "Calculating hash...",
  });

  const hashBuffer = await crypto.subtle.digest("SHA-256", arrayBuffer);
  const hashArray = Array.from(new Uint8Array(hashBuffer));
  const hashHex = hashArray
    .map((b) => b.toString(16).padStart(2, "0"))
    .join("");

  self.postMessage({
    type: "progress",
    progress: 100,
    message: "Hash calculated",
  });

  self.postMessage({
    type: "complete",
    data: {
      hash: hashHex,
      algorithm: "SHA-256",
    },
  });
}

// Helper function to format file size
function formatFileSize(bytes) {
  if (bytes === 0) return "0 Bytes";
  const k = 1024;
  const sizes = ["Bytes", "KB", "MB", "GB"];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
}
