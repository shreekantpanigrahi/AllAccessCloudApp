import { Component } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { UserProfileDto } from '../models/user-profile.model';
import { ProfileService } from '../services/profile.service';
import { AuthService } from '../services/auth.service';
import { Router, RouterModule } from '@angular/router';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [NgIf, ReactiveFormsModule, FormsModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent {
  profileForm!: FormGroup;
  passwordForm!: FormGroup;
  userProfile: UserProfileDto | null = null;
  previewUrl: string | null = null;
  uploading = false;
  saving = false;
  changingPassword = false;
  
  // State management for UI
  editingField: string | null = null;
  showPasswordSection = false;
  showLogoutSection = false;

  constructor(
    private fb: FormBuilder,
    private profileService: ProfileService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.initForms();
    this.loadProfile();
  }

  initForms(): void {
    this.profileForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]]
    });

    this.passwordForm = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  loadProfile(): void {
    this.profileService.getProfile().subscribe({
      next: (profile) => {
        this.userProfile = profile;
        this.profileForm.patchValue({
          name: profile.name || '',
          email: profile.email || ''
        });
        this.previewUrl = profile.profilePictureUrl || null;
      },
      error: (error) => {
        console.error('Error loading profile:', error);
      }
    });
  }

  getStoragePercent(): number {
    if (!this.userProfile) return 0; 
    return (this.userProfile.usedStorage / this.userProfile.storageQuota) * 100;
  }

  // Toggle edit mode for name/email fields
  toggleEditField(field: string): void {
    this.editingField = this.editingField === field ? null : field;
  }

  // Cancel editing
  cancelEdit(): void {
    this.editingField = null;
    // Reset form values to original profile data
    if (this.userProfile) {
      this.profileForm.patchValue({
        name: this.userProfile.name || '',
        email: this.userProfile.email || ''
      });
    }
  }

  // Toggle password section visibility
  togglePasswordSection(): void {
    this.showPasswordSection = !this.showPasswordSection;
    if (this.showPasswordSection) {
      this.passwordForm.reset();
    }
  }

  // Toggle logout section visibility
  toggleLogoutSection(): void {
    this.showLogoutSection = !this.showLogoutSection;
  }

  // onFileSelected(event: any): void {
  //   const file = event.target.files[0];
  //   if (!file) return;

  //   // Validate file type
  //   if (!file.type.startsWith('image/')) {
  //     alert('Please select an image file');
  //     return;
  //   }

  //   // Validate file size (max 5MB)
  //   if (file.size > 5 * 1024 * 1024) {
  //     alert('Image size should be less than 5MB');
  //     return;
  //   }

  //   // Show immediate preview using FileReader (frontend only)
  //   // const reader = new FileReader();
  //   // reader.onload = (e: any) => {
  //   //   this.previewUrl = e.target.result;
  //   //   console.log('Frontend preview URL set:', this.previewUrl);
  //   // };
  //   // reader.readAsDataURL(file);

  //   this.uploading = true;
  //   this.profileService.uploadProfilePicture(file).subscribe({
  //     next: (res: any) => {
  //       this.uploading = false;
  //       console.log('Backend response:', res);

  //       if(res.profilePictureUrl){
  //         this.previewUrl = res.profilePictureUrl;
  //         // Update the user profile data
  //         if (this.userProfile) {
  //           this.userProfile.profilePictureUrl = res.profilePictureUrl;
  //         }
  //         console.log('Profile picture updated successfully');
  //       }
  //       else{
  //         console.error('No profilePictureUrl in response');
  //         alert('Upload completed but no image URL returned');
  //       }
  //       // You might want to update the navbar profile picture here
  //       // this.authService.updateProfilePicture(res.profilePictureUrl);
  //     },
  //     error: (error) => {
  //       this.uploading = false;
  //       console.error('Upload failed:', error);
  //       if (error.status) {
  //         alert(`Upload failed: ${error.status} - ${error.message}`);
  //       } else {
  //       alert('Upload failed. Please check your connection and try again.');
  //       }
  //     }
  //   });
  // }

  onSave(): void {
    if (this.profileForm.invalid || !this.editingField) return;

    this.saving = true;
    this.profileService.updateProfile(this.profileForm.value).subscribe({
      next: (updatedProfile) => {
        this.saving = false;
        this.userProfile = { ...this.userProfile, ...updatedProfile };
        this.editingField = null;
        
        // Show success feedback (you can use a toast service instead)
        console.log('Profile updated successfully');
      },
      error: (error) => {
        this.saving = false;
        console.error('Update failed:', error);
        alert('Update failed. Please try again.');
      }
    });
  }

  onChangePassword(): void {
    if (this.passwordForm.invalid) return;

    this.changingPassword = true;
    this.profileService.changePassword(this.passwordForm.value).subscribe({
      next: () => {
        this.changingPassword = false;
        this.showPasswordSection = false;
        this.passwordForm.reset();
        
        // Show success message
        alert('Password changed successfully!');
      },
      error: (error) => {
        this.changingPassword = false;
        console.error('Password change failed:', error);
        alert('Current password is incorrect or something went wrong.');
      }
    });
  }

  getUserInitial(): string {
  if (this.userProfile?.name) {
    return this.userProfile.name.charAt(0).toUpperCase();
  }
  
  // Fallback to current user from auth service
  const user = this.authService.getCurrentUser();
  return user?.name?.charAt(0).toUpperCase() || 'U';
}

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/auth/login']);
  }
}