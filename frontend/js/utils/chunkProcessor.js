// Chunk processing utilities for large files

import { logger } from "./logger.js";

class ChunkProcessor {
  constructor() {
    this.chunkSize = 1024 * 1024; // 1MB default
  }

  // Create chunks from file
  createChunks(file, chunkSize = this.chunkSize) {
    const chunks = [];
    let offset = 0;
    const totalChunks = Math.ceil(file.size / chunkSize);

    logger.info(`Creating ${totalChunks} chunks from file: ${file.name}`);

    while (offset < file.size) {
      const end = Math.min(offset + chunkSize, file.size);
      const chunk = file.slice(offset, end);

      chunks.push({
        data: chunk,
        index: chunks.length,
        offset: offset,
        size: chunk.size,
        totalSize: file.size,
        totalChunks: totalChunks,
        filename: file.name,
        filetype: file.type,
        isLast: end >= file.size,
      });

      offset = end;
    }

    logger.info(`Created ${chunks.length} chunks successfully`);
    return chunks;
  }

  // Process chunk (can be extended for compression, encryption, etc.)
  async processChunk(chunk, options = {}) {
    try {
      logger.debug(`Processing chunk ${chunk.index + 1}/${chunk.totalChunks}`);

      // Convert chunk to array buffer if needed
      const arrayBuffer = await this.chunkToArrayBuffer(chunk.data);

      // Apply any processing (compression, encryption, etc.)
      let processedData = arrayBuffer;

      if (options.compress) {
        processedData = await this.compressChunk(processedData);
      }

      if (options.encrypt) {
        processedData = await this.encryptChunk(
          processedData,
          options.encryptionKey
        );
      }

      return {
        ...chunk,
        data: processedData,
        processed: true,
      };
    } catch (error) {
      logger.error(`Error processing chunk ${chunk.index}:`, error);
      throw error;
    }
  }

  // Reassemble chunks back into a file
  async reassembleChunks(chunks) {
    try {
      logger.info(`Reassembling ${chunks.length} chunks`);

      // Sort chunks by index to ensure correct order
      const sortedChunks = [...chunks].sort((a, b) => a.index - b.index);

      // Combine all chunk data
      const blobs = sortedChunks.map((chunk) => {
        if (chunk.data instanceof Blob) {
          return chunk.data;
        } else if (chunk.data instanceof ArrayBuffer) {
          return new Blob([chunk.data]);
        } else {
          throw new Error(`Invalid chunk data type at index ${chunk.index}`);
        }
      });

      // Create final blob
      const finalBlob = new Blob(blobs, {
        type: sortedChunks[0].filetype || "application/octet-stream",
      });

      // Create file from blob
      const file = new File([finalBlob], sortedChunks[0].filename, {
        type: sortedChunks[0].filetype,
      });

      logger.info(`Successfully reassembled file: ${file.name}`);
      return file;
    } catch (error) {
      logger.error("Error reassembling chunks:", error);
      throw error;
    }
  }

  // Convert chunk to array buffer
  async chunkToArrayBuffer(chunk) {
    if (chunk instanceof ArrayBuffer) {
      return chunk;
    }

    if (chunk instanceof Blob) {
      return await chunk.arrayBuffer();
    }

    throw new Error("Invalid chunk type");
  }

  // Compress chunk (placeholder - would need compression library)
  async compressChunk(arrayBuffer) {
    // TODO: Implement actual compression using a library like pako
    logger.warn("Compression not implemented, returning original data");
    return arrayBuffer;
  }

  // Encrypt chunk (placeholder - would need crypto library)
  async encryptChunk(arrayBuffer, key) {
    // TODO: Implement actual encryption using Web Crypto API
    logger.warn("Encryption not implemented, returning original data");
    return arrayBuffer;
  }

  // Calculate chunk hash for integrity verification
  async calculateChunkHash(chunk) {
    try {
      const arrayBuffer = await this.chunkToArrayBuffer(chunk.data);
      const hashBuffer = await crypto.subtle.digest("SHA-256", arrayBuffer);
      const hashArray = Array.from(new Uint8Array(hashBuffer));
      const hashHex = hashArray
        .map((b) => b.toString(16).padStart(2, "0"))
        .join("");
      return hashHex;
    } catch (error) {
      logger.error("Error calculating chunk hash:", error);
      return null;
    }
  }

  // Verify chunk integrity
  async verifyChunkIntegrity(chunk, expectedHash) {
    const actualHash = await this.calculateChunkHash(chunk);
    return actualHash === expectedHash;
  }

  // Get chunk progress info
  getChunkProgress(processedChunks, totalChunks) {
    const percentage = Math.round((processedChunks / totalChunks) * 100);
    return {
      processed: processedChunks,
      total: totalChunks,
      percentage: percentage,
      remaining: totalChunks - processedChunks,
    };
  }

  // Process all chunks with progress callback
  async processAllChunks(chunks, options = {}, onProgress = null) {
    const processedChunks = [];

    for (let i = 0; i < chunks.length; i++) {
      const processedChunk = await this.processChunk(chunks[i], options);
      processedChunks.push(processedChunk);

      if (onProgress) {
        const progress = this.getChunkProgress(i + 1, chunks.length);
        onProgress(progress);
      }
    }

    return processedChunks;
  }

  // Upload chunks sequentially
  async uploadChunksSequential(chunks, uploadFunction, onProgress = null) {
    const results = [];

    for (let i = 0; i < chunks.length; i++) {
      try {
        const result = await uploadFunction(chunks[i]);
        results.push(result);

        if (onProgress) {
          const progress = this.getChunkProgress(i + 1, chunks.length);
          onProgress(progress);
        }
      } catch (error) {
        logger.error(`Failed to upload chunk ${i}:`, error);
        throw error;
      }
    }

    return results;
  }

  // Upload chunks in parallel (with concurrency limit)
  async uploadChunksParallel(
    chunks,
    uploadFunction,
    concurrency = 3,
    onProgress = null
  ) {
    const results = new Array(chunks.length);
    let completed = 0;
    let index = 0;

    const uploadNext = async () => {
      if (index >= chunks.length) return;

      const currentIndex = index++;
      const chunk = chunks[currentIndex];

      try {
        const result = await uploadFunction(chunk);
        results[currentIndex] = result;
        completed++;

        if (onProgress) {
          const progress = this.getChunkProgress(completed, chunks.length);
          onProgress(progress);
        }

        // Upload next chunk
        await uploadNext();
      } catch (error) {
        logger.error(`Failed to upload chunk ${currentIndex}:`, error);
        throw error;
      }
    };

    // Start concurrent uploads
    const workers = Array(Math.min(concurrency, chunks.length))
      .fill(null)
      .map(() => uploadNext());

    await Promise.all(workers);
    return results;
  }
}

// Create global chunk processor instance
const chunkProcessor = new ChunkProcessor();

export default ChunkProcessor;
export { chunkProcessor };
