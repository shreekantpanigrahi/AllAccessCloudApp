import { NgIf } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { AlertService } from '../../services/alert.service';
import { Router } from '@angular/router';
import { ForgotPasswordModel } from '../../models/forgot-password.model';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [ReactiveFormsModule, NgIf],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.css'
})
export class ForgotPasswordComponent implements OnInit {
  form!: FormGroup;
  loading=false;

  constructor(private fb: FormBuilder, private authService: AuthService, private alertService: AlertService, private router:Router){}

  ngOnInit(): void{
    this.form= this.fb.group({
      email:['', [Validators.required,Validators.email]]
    });
  }

  get email(){
    return this.form.get('email')!;
  }

  onSubmit(): void{
    if(this.form.invalid) return;

    this.loading= true;
    const model: ForgotPasswordModel=this.form.value;

    this.authService.forgotPassword(model).subscribe({
      next:(res)=>{
        this.loading = false;
        this.alertService.success('Check Your Email', res.message || 'An OTP has been sent to your email.');
        this.router.navigate(['/auth/verify-otp'], { queryParams: { email: model.email } });
      },
      error:(err)=>{
        this.loading = false;
        this.alertService.error('Error', err.error?.message || 'Failed to send OTP');
      }
    })
  }
}
