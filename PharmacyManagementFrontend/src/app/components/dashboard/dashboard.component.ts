import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { User } from '../../models/auth.models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  currentUser: User | null = null;

  constructor(
    public authService: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
      console.log('Dashboard currentUser:', user);
      console.log('isAdmin:', this.authService.isAdmin());
      console.log('isDoctor:', this.authService.isDoctor());
    });
  }

  getUserName(): string {
    if (this.currentUser?.role) {
      return this.currentUser.role;
    }
    return 'User';
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}