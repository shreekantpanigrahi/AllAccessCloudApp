import { Component, EventEmitter, Input, Output, SimpleChanges } from '@angular/core';
import { UserProfileDto } from '../../../models/user-profile.model';
import { Router } from '@angular/router';
import { CommonModule, NgIf } from '@angular/common';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-profile-dropdown',
  standalone: true,
  imports: [CommonModule, NgIf],
  templateUrl: './profile-dropdown.component.html',
  styleUrl: './profile-dropdown.component.css'
})
export class ProfileDropdownComponent {
  @Input() profile: UserProfileDto | null = null;
  @Output() close = new EventEmitter<void>();
  constructor(private router: Router, private authService: AuthService) {}

  ngOnInit(): void {}

  getUserInitial(): string {
    if (this.profile?.name) {
      return this.profile.name.charAt(0).toUpperCase();
    }
    const user = this.authService.getCurrentUser();
    return user?.name?.charAt(0).toUpperCase() || 'U'; 
  }


  getStoragePercent(): number {
    if (!this.profile) return 0; 
    return (this.profile.usedStorage / this.profile.storageQuota) * 100;
  }

  onEdit(): void {
    this.close.emit();
    this.router.navigate(['/profile']);
  }
}
