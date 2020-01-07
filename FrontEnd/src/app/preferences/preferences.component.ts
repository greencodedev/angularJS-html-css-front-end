import { Component, OnInit } from '@angular/core';
import { SidebarService } from "../services/sidebar.service";

@Component({
  selector: 'app-preferences',
  templateUrl: './preferences.component.html',
  styleUrls: ['./preferences.component.scss']
})
export class PreferencesComponent implements OnInit {
  isOpened$ = this.sidebarService.isOpened$;

  constructor(private sidebarService: SidebarService) { }

  ngOnInit() {
  }

}
