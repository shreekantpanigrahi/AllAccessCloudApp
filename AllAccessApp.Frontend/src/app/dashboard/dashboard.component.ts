import { Component, ElementRef, ViewChild } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { FilesService } from '../services/files.service';
import { NgIf } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { AlertService } from '../services/alert.service';
import { FileUploadResponse } from '../models/file.model';
import { ProfileService } from '../services/profile.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [NgIf],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent {
  userName : string ="User";
  loading=false;
  uploadSuccess= false;
  uploadError='';

  @ViewChild('fileInput') fileInput!: ElementRef;

  constructor (private authService: AuthService, private fileService: FilesService, private router: Router, private alertService: AlertService){}

  ngOnInit():void{
    const user = this.authService.getCurrentUser();
    this.userName = user?.name || 'There';
  }

  openFileInput():void{
    this.fileInput.nativeElement.click();
  }

  onFileSelected(event: any): void{
    const file = event.target.files[0];
    if(!file) return;
    event.target.value = null;

    this.uploadFile(file);
  }

  private uploadFile(file:File): void{
    this.loading=true;
    this.uploadSuccess=false;
    this.uploadError='';
    this.alertService.loading('Uploading...');

    this.fileService.uploadFile(file).subscribe({
      next: async (response: FileUploadResponse)=>{
        this.loading = false;
        this.alertService.close();  // Close loading popup
        
        this.uploadSuccess = true;
        this.alertService.success('Uploaded!', `${response.fileName} was saved successfully.`);

        // try {
        //   await this.profileService.reloadProfile();
        //   console.log('Profile reloaded with updated storage');
        // } catch (err) {
        //   console.error('Failed to reload profile', err);
        // }
      },
      error:(err)=>{
        this.loading=false;
        this.alertService.close();

        this.uploadError= err.error?.message || 'Upload failed. Please try again.';
        const message = this.uploadError;

        this.alertService.error('Upload Failed', message);
      }
    });
  }

  handleUploadClick(): void{
    if(!this.authService.isLoggedIn()){
      this.alertService.warning('Login Required', 'Please log in to upload files.');

      this.router.navigate(['/auth/login'],{
        queryParams: {returnUrl: '/dashboard'}
      });
      return;
    }
    this.openFileInput();
  }
}
