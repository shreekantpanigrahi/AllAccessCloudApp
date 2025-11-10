import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ContactForm, ContactResponse } from '../models/file.model';

const API_URL='https://localhost:7064/api';

@Injectable({
  providedIn: 'root'
})
export class ContactService {

  constructor(private http:HttpClient) { }

  send(form : ContactForm): Observable<ContactResponse>{
    return this.http.post<ContactResponse>(`${API_URL}/contact`,form);
  }
}
