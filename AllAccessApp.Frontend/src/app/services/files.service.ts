import { Injectable } from '@angular/core';
import {HttpClient, HttpEvent, HttpResponse} from '@angular/common/http'
import {Observable} from 'rxjs'
import {FileUploadResponse, FileItemDto, FileDownloadResponse, FileListResponse} from '../models/file.model'
import { environment } from '../../environments/environment';


@Injectable({
  providedIn: 'root'
})
export class FilesService {
  private apiUrl=`${environment.apiUrl}/File`
  constructor(private http: HttpClient) { }

  uploadFile(file:File):Observable<FileUploadResponse>{
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<FileUploadResponse>(`${this.apiUrl}/upload`, formData);
  }

  getUserFiles(): Observable<FileListResponse> {
    return this.http.get<FileListResponse>(`${this.apiUrl}/my-files`);
  }

  downloadFile(fileId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${fileId}/download`, {
      responseType: 'blob' 
    });
  }

  viewFile(fileId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${fileId}/view`, {
      responseType: 'blob'
    });
  }

  deleteFile(fileId: number): Observable<FileItemDto> {
    return this.http.delete<FileItemDto>(`${this.apiUrl}/${fileId}/delete`);
  }
}
