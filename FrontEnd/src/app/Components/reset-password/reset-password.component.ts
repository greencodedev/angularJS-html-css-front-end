import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Alert } from 'selenium-webdriver';
import { ApiService } from 'src/app/services/api.service';
@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.css']
})
export class ResetPasswordComponent implements OnInit {

  vars = {
    Email: '',
    NewPassword: '',
    ConfirmPassword: '',
    Token: '',
  }

  constructor(private route: ActivatedRoute, private router: Router, private api: ApiService) { }

  ngOnInit() {
    this.vars.Email = decodeURI(this.route.snapshot.queryParamMap.get('Email'));
    this.vars.Token = decodeURI(this.route.snapshot.queryParamMap.get('Token'));

    if(!this.vars.Email || !this.vars.Token) {
      alert("Error:" + "The link is missing a token. You might have clicked a broken link")
    }

    // Remove params
    this.router.navigate([], {
      queryParams: {
        Email: null,
        Token: null,
      },
      queryParamsHandling: 'merge'
    });

    
  }


  submitReset() {
    var $this = this;

    this.api.resetPassword(this.vars, function(success){

      alert("Success:" + "Your password has been reset.");

      setTimeout(() => {
        $this.router.navigate(['/sign-in']);
      },2000)
      

    }, function(error){
      alert("Error: " + error.Messages);
    });
  }

  }


