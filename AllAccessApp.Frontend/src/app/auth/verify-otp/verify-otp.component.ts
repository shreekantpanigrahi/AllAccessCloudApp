import { NgIf } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { AlertService } from '../../services/alert.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ForgotPasswordModel, VerifyOtpModel } from '../../models/forgot-password.model';

@Component({
  selector: 'app-verify-otp',
  standalone: true,
  imports: [ReactiveFormsModule, NgIf],
  templateUrl: './verify-otp.component.html',
  styleUrl: './verify-otp.component.css'
})
export class VerifyOtpComponent implements OnInit, OnDestroy{
  form!:FormGroup;
  loading=false;
  email='';

  // 🔵 Timer properties
  resendTimer: number = 60;
  timerInterval: any;

  constructor( private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService,
    private alertService: AlertService){}

    ngOnInit():void{
      this.email=this.route.snapshot.queryParams['email'];
      if(!this.email){
        this.router.navigate(['/auth/forgot-password']);
      }
      this.form=this.fb.group({
        otp: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]]
      });
      // Start countdown
      this.startResendTimer();
    }

    get otp(){ return this.form.get('otp')!; }

    onSubmit():void{
      if(this.form.invalid) return;

      this.loading=true;
      const model: VerifyOtpModel= {
        email: this.email,
        otp: this.otp.value
      };

      this.authService.verifyOtp(model).subscribe({
        next: (res)=>{
          this.loading=false;
          this.alertService.success('Verified!', 'OTP verified successfully.');
          this.router.navigate(['/auth/reset-password'], { queryParams: { email: this.email } });
        },
        error:(err)=>{
          this.loading=false;
          this.alertService.error('Invalid OTP', err.error?.message || 'Please try again.');
        }
      });
    }

    ngOnDestroy(): void {
      if (this.timerInterval) {
        clearInterval(this.timerInterval);
      }
    }

    startResendTimer(): void {
      this.timerInterval = setInterval(() => {
        this.resendTimer--;

        if (this.resendTimer <= 0) {
          clearInterval(this.timerInterval);
        }
      }, 1000);
    }

    onResendOtp(): void {
      if (this.resendTimer > 0) return; // Prevent early click

      this.loading = true;
      const model: ForgotPasswordModel = { email: this.email };

      this.authService.forgotPassword(model).subscribe({
        next: (res) => {
          this.loading = false;
          this.alertService.success('Check Your Email', res.message || 'A new OTP has been sent.');
          
          // Reset timer
          this.resendTimer = 60;
          if (this.timerInterval) clearInterval(this.timerInterval);
          this.startResendTimer();
        },
        error: (err) => {
          this.loading = false;
          this.alertService.error('Error', err.error?.message || 'Failed to send OTP');
        }
      });
    }
}
