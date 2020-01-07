import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from './services/api.service';
import { CATCH_ERROR_VAR } from '@angular/compiler/src/output/abstract_emitter';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  
  Criteria = {
    TextSearch:null, 
    Conditions:null, 
    Name: null, 
    Id:null,
    //Status:false, 
    category:null

  };

  constructor(private api: ApiService, private router: Router) {

  }

  ngOnInit()  {


    
    // this.api.Sync(function(data){
    //   console.log('success', data);
    // }, function(error){
    //   alert(error.Messages.join(', '));
    // })

    // this.api.SearchCategory(function(data){
    //   console.log('success', data);
    // }, function(error){
    //   alert(error.Messages.join(', '));
    // });
 
    
    // this.api.SearchProducts(this.Criteria, function(data){

    //   console.log('success', data);

    // }, function(error){
    //   alert(error.Messages.join(', '));
    // });

  }

}
