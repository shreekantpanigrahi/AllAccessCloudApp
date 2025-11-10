import { NgIf } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { AlertService } from '../../services/alert.service';
import { ResetPasswordModel } from '../../models/forgot-password.model';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [ReactiveFormsModule, NgIf, RouterLink],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.css'
})
export class ResetPasswordComponent implements OnInit {
  form!: FormGroup;
  loading=false;
  email='';
  hidePassword:boolean=true;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService,
    private alertService: AlertService
  ) {}

  ngOnInit(): void {
    this.email = this.route.snapshot.queryParams['email'];
    if (!this.email) {
      this.router.navigate(['/auth/forgot-password']);
      return;
    }

    this.form = this.fb.group({
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  togglePassword(): void {
    this.hidePassword = !this.hidePassword;
  }

  get newPassword() { return this.form.get('newPassword')!; }
  get confirmPassword() { return this.form.get('confirmPassword')!; }

  passwordMatchValidator(form: FormGroup) {
    return form.get('newPassword')?.value === form.get('confirmPassword')?.value
      ? null : { mismatch: true };
  }

  onSubmit(): void{
    if(this.form.invalid) return;

    this.loading= true;
    const model: ResetPasswordModel ={
      email: this.email,
      newPassword: this.newPassword.value,
      confirmPassword: this.confirmPassword.value
    }

    this.authService.resetPassword(model).subscribe({
      next: (res) => {
        this.loading = false;
        this.alertService.success('Success!', res.message);
        this.router.navigate(['/auth/login']);
      },
      error: (err) => {
        this.loading = false;
        this.alertService.error('Error', err.error?.message || 'Failed to reset password');
      }
    });
  }

}
