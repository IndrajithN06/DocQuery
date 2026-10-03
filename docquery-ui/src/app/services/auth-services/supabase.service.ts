import { Injectable } from '@angular/core';
import { createClient, SupabaseClient } from '@supabase/supabase-js';
import { environment } from '../../../../environment';

@Injectable({
    providedIn: 'root'
})
export class SupabaseService {

    private supabase: SupabaseClient;

    constructor() {
        this.supabase = createClient(
            environment.supabaseUrl,
            environment.supabasePublishableKey
        );
    }

    async signUp(email: string, password: string) {
        return await this.supabase.auth.signUp({
            email,
            password
        });
    }

    async signIn(email: string, password: string) {
        return await this.supabase.auth.signInWithPassword({
            email,
            password
        });
    }

    async signOut() {
        return await this.supabase.auth.signOut();
    }

    async getSession() {
        return await this.supabase.auth.getSession();
    }

    onAuthStateChange(callback: (session: any) => void) {
        return this.supabase.auth.onAuthStateChange(
            (_event, session) => {
                callback(session);
            }
        );
    }
}
