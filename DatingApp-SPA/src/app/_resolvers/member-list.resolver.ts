import { Injectable } from "@angular/core";
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';
import { User } from '../_models/user';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/alertify.service';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable()
export class MemberListResolver implements Resolve<User[]> {
    pageNumber = 1;
    pageSize = 5;

    constructor(private userSerivce: UserService,
        private route: Router, private alertify: AlertifyService){}

        resolve(route: ActivatedRouteSnapshot) : Observable<User[]>{
            return this.userSerivce.getUsers(this.pageNumber, this.pageSize).pipe(
                catchError(error => {
                    this.alertify.error('Problem retrieving data');
                    this.route.navigate(['/home']);
                    return of(null);
                })
            );
        }
}