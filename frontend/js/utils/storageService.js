// Storage Service with Locking and Security
// Provides thread-safe storage operations with sessionStorage preference

class StorageService {
  constructor() {
    this.locks = new Map();
    this.useSessionStorage = true; // More secure than localStorage
    this.encryptionEnabled = false;
    this.encryptionKey = null;
  }

  /**
   * Acquire a lock for a storage key
   * @param {string} key - Storage key
   * @returns {Promise<void>} Promise that resolves when lock is acquired
   */
  async acquireLock(key) {
    return new Promise((resolve) => {
      const checkLock = () => {
        if (!this.locks.has(key)) {
          this.locks.set(key, true);
          resolve();
        } else {
          // Wait 10ms and try again
          setTimeout(checkLock, 10);
        }
      };
      checkLock();
    });
  }

  /**
   * Release a lock for a storage key
   * @param {string} key - Storage key
   */
  releaseLock(key) {
    this.locks.delete(key);
  }

  /**
   * Get the appropriate storage object
   * @returns {Storage} sessionStorage or localStorage
   */
  getStorage() {
    try {
      return this.useSessionStorage ? sessionStorage : localStorage;
    } catch (error) {
      // Fallback to localStorage if sessionStorage fails
      console.warn(
        "SessionStorage not available, falling back to localStorage"
      );
      return localStorage;
    }
  }

  /**
   * Encrypt data before storage
   * @param {string} data - Data to encrypt
   * @returns {string} Encrypted data
   */
  encrypt(data) {
    if (!this.encryptionEnabled || !this.encryptionKey) {
      return data;
    }

    // Simple XOR encryption (not cryptographically secure, but better than plain text)
    let encrypted = "";
    for (let i = 0; i < data.length; i++) {
      encrypted += String.fromCharCode(
        data.charCodeAt(i) ^
          this.encryptionKey.charCodeAt(i % this.encryptionKey.length)
      );
    }
    return btoa(encrypted); // Base64 encode
  }

  /**
   * Decrypt data after retrieval
   * @param {string} data - Encrypted data
   * @returns {string} Decrypted data
   */
  decrypt(data) {
    if (!this.encryptionEnabled || !this.encryptionKey) {
      return data;
    }

    try {
      const decoded = atob(data); // Base64 decode
      let decrypted = "";
      for (let i = 0; i < decoded.length; i++) {
        decrypted += String.fromCharCode(
          decoded.charCodeAt(i) ^
            this.encryptionKey.charCodeAt(i % this.encryptionKey.length)
        );
      }
      return decrypted;
    } catch (error) {
      console.warn("Failed to decrypt data, returning as-is");
      return data;
    }
  }

  /**
   * Get an item from storage
   * @param {string} key - Storage key
   * @returns {Promise<string|null>} Stored value or null
   */
  async getItem(key) {
    await this.acquireLock(key);
    try {
      const storage = this.getStorage();
      const value = storage.getItem(key);
      return value ? this.decrypt(value) : null;
    } catch (error) {
      console.error("Error getting item from storage:", error);
      return null;
    } finally {
      this.releaseLock(key);
    }
  }

  /**
   * Set an item in storage
   * @param {string} key - Storage key
   * @param {string} value - Value to store
   * @returns {Promise<boolean>} Success status
   */
  async setItem(key, value) {
    await this.acquireLock(key);
    try {
      const storage = this.getStorage();
      const encryptedValue = this.encrypt(value);
      storage.setItem(key, encryptedValue);
      return true;
    } catch (error) {
      console.error("Error setting item in storage:", error);
      return false;
    } finally {
      this.releaseLock(key);
    }
  }

  /**
   * Remove an item from storage
   * @param {string} key - Storage key
   * @returns {Promise<boolean>} Success status
   */
  async removeItem(key) {
    await this.acquireLock(key);
    try {
      const storage = this.getStorage();
      storage.removeItem(key);
      return true;
    } catch (error) {
      console.error("Error removing item from storage:", error);
      return false;
    } finally {
      this.releaseLock(key);
    }
  }

  /**
   * Clear all items from storage
   * @returns {Promise<boolean>} Success status
   */
  async clear() {
    try {
      const storage = this.getStorage();
      storage.clear();
      return true;
    } catch (error) {
      console.error("Error clearing storage:", error);
      return false;
    }
  }

  /**
   * Get all keys from storage
   * @returns {Promise<string[]>} Array of storage keys
   */
  async keys() {
    try {
      const storage = this.getStorage();
      const keys = [];
      for (let i = 0; i < storage.length; i++) {
        keys.push(storage.key(i));
      }
      return keys;
    } catch (error) {
      console.error("Error getting storage keys:", error);
      return [];
    }
  }

  /**
   * Get storage usage information
   * @returns {Promise<Object>} Storage usage stats
   */
  async getUsage() {
    try {
      const storage = this.getStorage();
      let totalSize = 0;
      const items = {};

      for (let i = 0; i < storage.length; i++) {
        const key = storage.key(i);
        const value = storage.getItem(key);
        const size = (key.length + value.length) * 2; // Approximate size in bytes
        totalSize += size;
        items[key] = size;
      }

      return {
        totalSize,
        itemCount: storage.length,
        items,
        storageType: this.useSessionStorage ? "sessionStorage" : "localStorage",
      };
    } catch (error) {
      console.error("Error getting storage usage:", error);
      return { totalSize: 0, itemCount: 0, items: {}, storageType: "unknown" };
    }
  }

  /**
   * Store JSON data
   * @param {string} key - Storage key
   * @param {any} data - Data to store
   * @returns {Promise<boolean>} Success status
   */
  async setJSON(key, data) {
    try {
      const jsonString = JSON.stringify(data);
      return await this.setItem(key, jsonString);
    } catch (error) {
      console.error("Error storing JSON data:", error);
      return false;
    }
  }

  /**
   * Retrieve JSON data
   * @param {string} key - Storage key
   * @param {any} defaultValue - Default value if key doesn't exist
   * @returns {Promise<any>} Parsed JSON data or default value
   */
  async getJSON(key, defaultValue = null) {
    try {
      const jsonString = await this.getItem(key);
      if (jsonString === null) {
        return defaultValue;
      }
      return JSON.parse(jsonString);
    } catch (error) {
      console.error("Error retrieving JSON data:", error);
      return defaultValue;
    }
  }

  /**
   * Check if a key exists in storage
   * @param {string} key - Storage key
   * @returns {Promise<boolean>} Whether key exists
   */
  async hasItem(key) {
    const value = await this.getItem(key);
    return value !== null;
  }

  /**
   * Set storage preference
   * @param {boolean} useSessionStorage - Whether to use sessionStorage
   */
  setStoragePreference(useSessionStorage) {
    this.useSessionStorage = useSessionStorage;
  }

  /**
   * Enable encryption for sensitive data
   * @param {string} key - Encryption key
   */
  enableEncryption(key) {
    this.encryptionEnabled = true;
    this.encryptionKey = key;
  }

  /**
   * Disable encryption
   */
  disableEncryption() {
    this.encryptionEnabled = false;
    this.encryptionKey = null;
  }

  /**
   * Migrate data from localStorage to sessionStorage or vice versa
   * @param {boolean} toSessionStorage - Whether to migrate to sessionStorage
   * @returns {Promise<boolean>} Success status
   */
  async migrateStorage(toSessionStorage) {
    try {
      const sourceStorage = toSessionStorage ? localStorage : sessionStorage;
      const targetStorage = toSessionStorage ? sessionStorage : localStorage;

      // Get all items from source
      const items = {};
      for (let i = 0; i < sourceStorage.length; i++) {
        const key = sourceStorage.key(i);
        items[key] = sourceStorage.getItem(key);
      }

      // Store in target
      for (const [key, value] of Object.entries(items)) {
        targetStorage.setItem(key, value);
      }

      // Clear source
      sourceStorage.clear();

      this.useSessionStorage = toSessionStorage;
      return true;
    } catch (error) {
      console.error("Error migrating storage:", error);
      return false;
    }
  }

  /**
   * Clean up expired items (if using TTL)
   * @param {number} maxAge - Maximum age in milliseconds
   * @returns {Promise<number>} Number of items cleaned up
   */
  async cleanup(maxAge = 24 * 60 * 60 * 1000) {
    // 24 hours default
    try {
      const storage = this.getStorage();
      const now = Date.now();
      let cleanedCount = 0;

      for (let i = storage.length - 1; i >= 0; i--) {
        const key = storage.key(i);
        const value = storage.getItem(key);

        try {
          const data = JSON.parse(value);
          if (data.timestamp && now - data.timestamp > maxAge) {
            storage.removeItem(key);
            cleanedCount++;
          }
        } catch {
          // Not a timestamped item, skip
        }
      }

      return cleanedCount;
    } catch (error) {
      console.error("Error cleaning up storage:", error);
      return 0;
    }
  }
}

// Create global storage service instance
const storageService = new StorageService();

// Export for module use
export { storageService, StorageService };

// Make available globally for easy access
window.storageService = storageService;
