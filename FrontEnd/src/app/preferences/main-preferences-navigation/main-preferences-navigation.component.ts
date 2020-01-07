import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { SidebarService } from "../../services/sidebar.service";

@Component({
  selector: 'app-main-preferences-navigation',
  templateUrl: './main-preferences-navigation.component.html',
  styleUrls: ['./main-preferences-navigation.component.css']
})
export class MainPreferencesNavigationComponent implements OnInit {
  isOpened$ = this.sidebarService.isOpened$;

  constructor(private router: Router, private sidebarService: SidebarService) { }

  ngOnInit() {
  }

}
