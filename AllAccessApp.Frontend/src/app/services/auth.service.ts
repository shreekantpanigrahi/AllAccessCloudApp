import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { AuthResponse, LoginModel, RegisterModel } from '../models/auth.model';
import { ForgotPasswordModel, ResetPasswordModel, VerifyOtpModel } from '../models/forgot-password.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})

export class AuthService {

  private apiUrl= `${environment.apiUrl}/Auth`;
  private currentUserSubject= new BehaviorSubject<any>(null);

  constructor(private http: HttpClient) { 
    const user = localStorage.getItem('user');
    if(user){
      this.currentUserSubject.next(JSON.parse(user));
    }
  }

  login(model:LoginModel):Observable<AuthResponse>{
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, model).pipe(
      tap(res=>{
        if(res.success && res.token){
          const user ={
            userId: res.userId,
            email:res.email,
            name:res.name,
            token: res.token
          };

          localStorage.setItem('user',JSON.stringify(user));
          this.currentUserSubject.next(user);
        }
      })
    );
  }

  register(model:RegisterModel):Observable<AuthResponse>{
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, model)
  }

  logout(){
    localStorage.removeItem('user');
    this.currentUserSubject.next(null)
  }

  getCurrentUser(){
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
  }

  isLoggedIn():boolean{
    const user= localStorage.getItem('user');
    return !!user;
  }

  forgotPassword(model: ForgotPasswordModel): Observable<AuthResponse>{
    return this.http.post<AuthResponse>(`${this.apiUrl}/forgot-password`, model);
  }

  verifyOtp(model: VerifyOtpModel): Observable<AuthResponse>{
    return this.http.post<AuthResponse>(`${this.apiUrl}/verify-otp`, model);
  }

  resetPassword(model: ResetPasswordModel):Observable<AuthResponse>{
    return this.http.post<AuthResponse>(`${this.apiUrl}/reset-password`, model);
  }
  
}
