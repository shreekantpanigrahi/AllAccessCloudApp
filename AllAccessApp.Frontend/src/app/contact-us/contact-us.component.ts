import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ContactService } from '../services/contact.service';
import { NgIf } from '@angular/common';
import { ContactForm } from '../models/file.model';
import { AlertService } from '../services/alert.service';

@Component({
  selector: 'app-contact-us',
  standalone: true,
  imports: [ReactiveFormsModule, NgIf],
  templateUrl: './contact-us.component.html',
  styleUrl: './contact-us.component.css'
})
export class ContactUsComponent implements OnInit{
  contactForm!: FormGroup;
  loading = false;
  success = false;
  error = '';

   constructor(private fb: FormBuilder, private http: HttpClient, private contactService: ContactService, private alertService: AlertService) {}

  ngOnInit(): void {
    this.contactForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      message: ['', Validators.required]
    });
  }

  get name(){return this.contactForm.get('name')!;}
  get email(){return this.contactForm.get('email')!;}
  get message(){return this.contactForm.get('message')!;}

  onSubmit():void{
    if(this.contactForm.invalid) return;

    this.loading=true;
    this.success=false;
    this.error='';

    const form: ContactForm = this.contactForm.value;

    this.contactService.send(form).subscribe({
      next: (response) => {
        this.loading = false;
        this.success = true;
        this.contactForm.reset();
        this.alertService.success('Message sent:', response.message);

        // Auto-hide success message after 5 seconds
        // setTimeout(() => this.success = false, 5000);
      },
      error: (err) => {
        this.loading = false;
        this.error = err.error?.message || 'Failed to send message. Please try again later.';
        const message = this.error;
        this.alertService.error('Send failed', message);
      }
    });
  }
}
