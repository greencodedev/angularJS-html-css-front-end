import { Component, OnInit } from '@angular/core';
import { ApiService } from 'src/app/services/api.service';
import { Router } from '@angular/router';
import { SidebarService } from "../../services/sidebar.service";
import { startWith } from "rxjs/operators";

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent implements OnInit {
  constructor(private api: ApiService,private router: Router, private sidebarService: SidebarService) { }

  isSidebarOpened$ = this.sidebarService.isOpened$;
  isHamburgerShown$ = this.sidebarService.isHamburgerShown$.pipe(
    startWith(this.sidebarService.isPageWithHamburger(this.router.url))
  );

  User: any = {};
  ngOnInit() {
    this.getCurrentUsers()
  }

  toggleSidebar() {
    this.sidebarService.toggle();
  }

  getCurrentUsers(){
    this.api.GetCurrentUsers(
      data => {
        this.User = data;
      },
    error => console.log(error)
    )
  }
  LogOut(){
    this.api.LogOut()
    
  }

}
