import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, firstValueFrom, Observable, tap } from 'rxjs';
import { ChangePassword, UpdateProfileModel, UserProfileDto } from '../models/user-profile.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ProfileService {
  private apiUrl= `${environment.apiUrl}/Profile`;

  constructor(private http: HttpClient) { }

  private profileSubject = new BehaviorSubject<UserProfileDto|null>(null);
  public profile$= this.profileSubject.asObservable();

  getProfile(): Observable<UserProfileDto>{
    return this.http.get<UserProfileDto>(this.apiUrl).pipe(
      tap(profile => this. profileSubject.next(profile))
    );
  }

  async reloadProfile(): Promise<UserProfileDto>{
    const profile = await firstValueFrom(this.getProfile());
    return profile;
  }

  updateProfile(model: UpdateProfileModel): Observable<any>{
    return this.http.put<UpdateProfileModel>(this.apiUrl, model);
  }

  changePassword(model: ChangePassword): Observable<any>{
    return this.http.post<ChangePassword>(`${this.apiUrl}/change-password`, model);
  }

  uploadProfilePicture(file: File): Observable<any>{
    const formData = new FormData();
    formData.append('file',file);
    return this.http.post(`${this.apiUrl}/upload-picture`,formData);
  }
}
