import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ContactForm, ContactResponse } from '../models/file.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ContactService {
  private apiUrl= `${environment.apiUrl}/contact`;
  constructor(private http:HttpClient) { }

  send(form : ContactForm): Observable<ContactResponse>{
    return this.http.post<ContactResponse>(`${this.apiUrl}`,form);
  }
}
