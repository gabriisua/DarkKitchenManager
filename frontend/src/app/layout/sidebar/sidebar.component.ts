import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatListItem } from '@angular/material/list';
import { MatIcon } from '@angular/material/icon';
import { MatExpansionPanel, MatExpansionPanelHeader, MatExpansionPanelTitle } from '@angular/material/expansion';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule, MatListItem, MatIcon, MatExpansionPanel, MatExpansionPanelHeader, MatExpansionPanelTitle],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss'
})
export class SidebarComponent implements OnInit {
  private router = inject(Router);

  readonly isCucinaOpen = signal(false);
  readonly isUtenzeOpen = signal(false);
  readonly isOrdiniFattureOpen = signal(false);

  ngOnInit() {
    const currentUrl = this.router.url;

    if (
      currentUrl.includes('ingredients') ||
      currentUrl.includes('plates') ||
      currentUrl.includes('categories') ||
      currentUrl.includes('allergens') ||
      currentUrl.includes('menus')
    ) {
      this.isCucinaOpen.set(true);
    }

    if (
      currentUrl.includes('staff') ||
      currentUrl.includes('customers')
    ) {
      this.isUtenzeOpen.set(true);
    }

    if (
      currentUrl.includes('sales') ||
      currentUrl.includes('orders') ||
      currentUrl.includes('invoices') ||
      currentUrl.includes('invoice-history')
    ) {
      this.isOrdiniFattureOpen.set(true);
    }
  }
}
