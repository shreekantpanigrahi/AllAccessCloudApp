import { Component, OnInit } from '@angular/core';
import { NgIf } from '@angular/common'; 
import { Router, RouterLink } from "@angular/router";
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { ActivatedRoute } from '@angular/router';
import { AlertService } from '../../services/alert.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [NgIf, RouterLink, FormsModule,ReactiveFormsModule ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent implements OnInit{
  loginForm!: FormGroup; 
  loading= false;
  hidePassword:boolean = true;

  constructor(private fb: FormBuilder, private authService: AuthService, private router: Router,  private route: ActivatedRoute,  private alertService: AlertService){
  }

  ngOnInit(): void {
    this.loginForm= this.fb.group({
      email:['',[Validators.email,Validators.required]],
      password:['',[Validators.required]]
    });
    const url = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
    if (this.authService.isLoggedIn()) {
      this.router.navigate([url]);
    }
  }

  togglePassword(): void {
    this.hidePassword = !this.hidePassword;
  }

  onSubmit(){
    if(this.loginForm.invalid) return;

    this.loading=true;
    const model= this.loginForm.value;
    this.alertService.loading('Logging in....');

    this.authService.login(model).subscribe({
      next:(res)=>{
        this.alertService.close();
        this.loading=false;
        if(res.success){
          this.alertService.success('Welcome back!', `Hi ${res.name}`);
          const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
          this.router.navigate([returnUrl]);
        }
      },
      error:()=>{
        this.loading=false;
        this.alertService.close();
        this.alertService.error('Login Failed', 'Please check your credentials');
      }
    })
  }
}
