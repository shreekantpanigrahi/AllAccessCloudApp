import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from "@angular/forms";
import { NgIf } from '@angular/common'; 
import { AuthService } from '../../services/auth.service';
import { Router, RouterLink } from '@angular/router';
import { AlertService } from '../../services/alert.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, NgIf, ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit{
  registerForm!:FormGroup;
  loading= false;
  hidePassword:boolean = true;

  constructor(private fb : FormBuilder, private authService:AuthService, private router: Router, private alertService: AlertService){}

  ngOnInit():void{
    this.registerForm= this.fb.group({
      name:['',Validators.required],
      email:['',[Validators.required, Validators.email]],
      password:['',[Validators.required, Validators.minLength(6)]]
    })
  }

  togglePassword(): void {
    this.hidePassword = !this.hidePassword;
  }

  onSubmit(): void{
    if(this.registerForm.invalid) return;

    this.loading=true;
    const model= this.registerForm.value;
    this.alertService.loading('Registering...');

    this.authService.register(model).subscribe({
      next:(res)=>{
        this.loading=false;
        if(res.success){
          this.alertService.success('Registration successful!!! You can log in');
          this.router.navigate(['/auth/login']);
        }else{
          this.alertService.error(res.message || 'Registration failed');
        }
      },
      error:()=>{
        this.loading=false;
        this.alertService.close();
        this.alertService.error('Registration failed, Try again');
      }
    })
  }
}
