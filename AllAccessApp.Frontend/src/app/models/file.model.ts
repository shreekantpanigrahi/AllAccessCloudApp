export interface FileUploadResponse{
    success: boolean;
    message?: string;
    fileName?: string;
    filePath?: string;
    fileSize: number;
    originalFileSize: number;
    fileType?: string;
    fileId: number;
    isCompressed: boolean;
    compressionRatio: number;
}


export interface FileItemDto{
    id: number;
    fileName: string;
    originalName: string;
    fileType: string;
    fileSize: number;
    originalFileSize: number;
    isCompressed: boolean;
    createdOn: string; // ISO date string
    modifiedOn: string; // ISO date string
    formattedSize: string;
    formattedOriginalSize: string;
}

export interface FileDownloadResponse {
  success: boolean;
  message?: string;
  fileName?: string;
  contentType?: string;
  fileSize: number;
}

export interface FileListResponse {
  success: boolean;
  message?: string;
  files?: FileItemDto[];
}

export interface ContactForm {
  name: string;
  email: string;
  message: string;
}

export interface ContactResponse {
  message: string; 
}

