import { Injectable } from '@angular/core';
import {HttpClient, HttpEvent, HttpResponse} from '@angular/common/http'
import {Observable} from 'rxjs'
import {FileUploadResponse, FileItemDto, FileDownloadResponse, FileListResponse} from '../models/file.model'

const API_URL='https://localhost:7064/api/File';

@Injectable({
  providedIn: 'root'
})
export class FilesService {

  constructor(private http: HttpClient) { }

  uploadFile(file:File):Observable<FileUploadResponse>{
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<FileUploadResponse>(`${API_URL}/upload`, formData);
  }

  getUserFiles(): Observable<FileListResponse> {
    return this.http.get<FileListResponse>(`${API_URL}/my-files`);
  }

  downloadFile(fileId: number): Observable<Blob> {
    return this.http.get(`${API_URL}/${fileId}/download`, {
      responseType: 'blob' 
    });
  }

  viewFile(fileId: number): Observable<Blob> {
    return this.http.get(`${API_URL}/${fileId}/view`, {
      responseType: 'blob'
    });
  }

  deleteFile(fileId: number): Observable<FileItemDto> {
    return this.http.delete<FileItemDto>(`${API_URL}/${fileId}/delete`);
  }
}
