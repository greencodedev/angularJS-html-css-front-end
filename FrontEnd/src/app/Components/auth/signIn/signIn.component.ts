import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../../services/api.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-signIn',
  templateUrl: './signIn.component.html',
  styleUrls: ['./signIn.component.css']
})
export class SignInComponent implements OnInit {

  UserName = '';
  Password = '';
  return = '';
 syncing = false;
  constructor(private api: ApiService, private route: ActivatedRoute, private router: Router) {
  }

  ngOnInit() {
    this.route.queryParams.subscribe(params => this.return = params['return'] || '/');
  }

  login() {
    let $this = this;
    this.syncing = true;
    this.api.login(this.UserName, this.Password, data => {
      $this.syncing = false;
      $this.router.navigate([this.return]);
    }, function (error) {
      $this.syncing = false;
      alert(error.error_description);
    });
  }


}

