import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Router, RouterModule } from '@angular/router';
import { NgIf } from '@angular/common';
import { UserProfileDto } from '../../models/user-profile.model';
import { ProfileService } from '../../services/profile.service';
import { ProfileDropdownComponent } from "../components/profile-dropdown/profile-dropdown.component";
import { AlertService } from '../../services/alert.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [NgIf, RouterModule, ProfileDropdownComponent],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent implements OnInit{
  isDropdownOpen = false;
  userProfile: UserProfileDto | null = null;
  showImage = false;
  profilePictureUrl = '';
  
  @ViewChild('dropdownContainer') container!: ElementRef;
  
  constructor(private authService: AuthService, private router: Router, private profileService: ProfileService, private alertService: AlertService) {
    if(this.authService.isLoggedIn()){
      this.loadProfile();
    }
  }

  ngOnInit(): void{
    this.profileService.profile$.subscribe(profile => {
      this.userProfile = profile;
      this.showImage = !!profile?.profilePictureUrl;
      this.profilePictureUrl = profile?.profilePictureUrl || '';
    });
  }

  loadProfile():void{
    this.profileService.getProfile().subscribe({
      next:(profile)=>{
        this.userProfile = profile;
        this.showImage = !!profile.profilePictureUrl;
        this.profilePictureUrl = profile.profilePictureUrl || '';
      },
      error: ()=>{
        console.log('Failed to load profile');
      }
    });
  }

  toggleDropdown(): void{
    this.isDropdownOpen=!this.isDropdownOpen;
    if (this.isDropdownOpen) {
      document.addEventListener('click', this.closeDropdownOnClickOutside);
    } else {
      document.removeEventListener('click', this.closeDropdownOnClickOutside);
    }
  }

  closeDropdownOnClickOutside = (event: Event): void => {
    if (!this.container.nativeElement.contains(event.target)) {
      this.isDropdownOpen = false;
      document.removeEventListener('click', this.closeDropdownOnClickOutside);
    }
  };
 
  ngOnDestroy(): void {
    document.removeEventListener('click', this.closeDropdownOnClickOutside);
  }

  showNavbar(): boolean {
    return this.authService.isLoggedIn();
  }

   getUserInitial(): string {
    const user = this.authService.getCurrentUser();
    return user?.name?.charAt(0).toUpperCase() || 'U';
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/auth/login']);
  }

  isAuthRoute(): boolean {
    return this.authService.isLoggedIn();
  }
}
